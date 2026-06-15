import { spawnSync } from "node:child_process";
import { mkdtempSync } from "node:fs";
import { tmpdir } from "node:os";
import express from "express";
import { query } from "@anthropic-ai/claude-agent-sdk";

// TaskManager agent sidecar (PoC).
//
// Pipeline: implement -> build+test gate (FR6) -> reviewer / LLM-as-judge (FR7)
// -> DRAFT PR. PR comments re-trigger the same pipeline on the existing branch
// (FR8). The .NET API never runs the agent in-process; this service does.
// Guardrails (../.specify/memory/constitution.md): scoped tools, task/{id} branch,
// draft PR only, never auto-merge. Evidence base: docs/cli-deployment-strategy.md §8.

const PORT = process.env.PORT || 8787;
const BUILD_TARGET = process.env.BUILD_TARGET || "TaskManager.Server.sln";
const BUILD_CMD = (process.env.BUILD_CMD || `dotnet build ${BUILD_TARGET} --nologo -v q`).split(" ");
const TEST_CMD = (process.env.TEST_CMD || `dotnet test ${BUILD_TARGET} --nologo -v q`).split(" ");
const MAX_FIX_ATTEMPTS = Number(process.env.MAX_FIX_ATTEMPTS || 3); // FR8: iteration cap
const COST_CAP_USD = Number(process.env.COST_CAP_USD || 2.0); // FR8: cost ceiling
const ALLOWED_TOOLS = ["Read", "Grep", "Glob", "Edit", "Write", "Bash"];
const SYSTEM_PROMPT =
  "You are a senior engineer. Obey the repository constitution at " +
  ".specify/memory/constitution.md. Produce a focused, reviewable change.";

const app = express();
app.use(express.json());
app.get("/health", (_req, res) => res.json({ status: "ok" }));

// Initial run: implement a task and open a draft PR.
app.post("/run", (req, res) => {
  const { taskId, name, description, repositoryUrl } = req.body ?? {};
  if (!taskId || !name || !description || !repositoryUrl) {
    return res.status(400).json({ error: "taskId, name, description and repositoryUrl are required" });
  }
  res.status(202).json({ status: "accepted", taskId });
  runInitial({ taskId, name, description, repositoryUrl }).catch((err) =>
    console.error(`[task ${taskId}] run failed:`, err),
  );
});

// FR8: a PR-review comment re-triggers work on the existing branch.
app.post("/feedback", (req, res) => {
  const { taskId, repositoryUrl, branch, name, comments } = req.body ?? {};
  if (!taskId || !repositoryUrl || !branch || !comments) {
    return res.status(400).json({ error: "taskId, repositoryUrl, branch and comments are required" });
  }
  res.status(202).json({ status: "accepted", taskId });
  runFeedback({ taskId, repositoryUrl, branch, name: name || branch, comments }).catch((err) =>
    console.error(`[task ${taskId}] feedback run failed:`, err),
  );
});

async function runInitial({ taskId, name, description, repositoryUrl }) {
  const branch = `task/${taskId}`;
  const workdir = prepareWorkspace(repositoryUrl, branch, { create: true });
  const budget = { spent: 0, cap: COST_CAP_USD };

  await runSession(taskId, workdir, budget, implementPrompt(name, description));

  if (!(await gate(taskId, workdir, budget))) {
    console.warn(`[task ${taskId}] not green ($${budget.spent.toFixed(2)} spent); NOT opening PR (FR6)`);
    return;
  }
  const review = await reviewPatch(taskId, workdir, budget, name, description);
  commitAndPush(workdir, branch, `Agent implementation for: ${name}`);
  openDraftPullRequest(workdir, name, taskId, review, budget);
}

async function runFeedback({ taskId, repositoryUrl, branch, name, comments }) {
  const workdir = prepareWorkspace(repositoryUrl, branch, { create: false });
  const budget = { spent: 0, cap: COST_CAP_USD };

  await runSession(taskId, workdir, budget, feedbackPrompt(comments));

  if (!(await gate(taskId, workdir, budget))) {
    console.warn(`[task ${taskId}] feedback not green; leaving branch as-is for review`);
    return;
  }
  const review = await reviewPatch(taskId, workdir, budget, name, comments);
  if (!commitAndPush(workdir, branch, `Address review feedback for: ${name}`)) {
    console.log(`[task ${taskId}] feedback produced no changes`);
    return;
  }
  // PR already exists for this branch — just add a comment with the new review.
  spawnSync("gh", ["pr", "comment", branch, "--body", `Addressed feedback.\n\n## Reviewer (LLM-as-judge)\n${review}`], {
    cwd: workdir,
    stdio: "inherit",
  });
}

// FR6: build + test gate with fix iterations. Returns true when green.
async function gate(taskId, workdir, budget) {
  for (let attempt = 0; attempt <= MAX_FIX_ATTEMPTS; attempt++) {
    const check = buildAndTest(workdir);
    if (check.ok) return true;
    if (attempt === MAX_FIX_ATTEMPTS || budget.spent >= budget.cap) return false;
    console.log(`[task ${taskId}] ${check.phase} failed (attempt ${attempt + 1}); asking agent to fix`);
    await runSession(taskId, workdir, budget, fixPrompt(check.phase, check.output));
  }
  return false;
}

async function runSession(taskId, cwd, budget, prompt) {
  for await (const message of query({
    prompt,
    options: { cwd, permissionMode: "acceptEdits", allowedTools: ALLOWED_TOOLS, systemPrompt: SYSTEM_PROMPT },
  })) {
    if (message.type === "result" && message.total_cost_usd != null) {
      budget.spent += message.total_cost_usd; // observability: per-task cost
    }
    console.log(`[task ${taskId}]`, JSON.stringify(message));
  }
}

// FR7: read the diff and produce a confidence verdict (read-only tools, no edits).
async function reviewPatch(taskId, cwd, budget, name, context) {
  const diff = git(cwd, ["diff", "HEAD"]).stdout;
  let verdict = "";
  for await (const message of query({
    prompt:
      `Act as an independent reviewer (LLM-as-judge) for this change.\n` +
      `Task: ${name}\n${context}\n\nDiff:\n${tail(diff, 12000)}\n\n` +
      `Respond with CONFIDENCE (low/medium/high), a one-paragraph SUMMARY, and RISKS.`,
    options: { cwd, permissionMode: "default", allowedTools: ["Read", "Grep", "Glob"] },
  })) {
    if (message.type === "result") {
      if (message.total_cost_usd != null) budget.spent += message.total_cost_usd;
      if (typeof message.result === "string") verdict = message.result;
    }
  }
  console.log(`[task ${taskId}] review:\n${verdict}`);
  return verdict || "Reviewer produced no verdict.";
}

function buildAndTest(workdir) {
  const b = run(workdir, BUILD_CMD);
  if (b.status !== 0) return { ok: false, phase: "build", output: tail((b.stdout || "") + (b.stderr || "")) };
  const t = run(workdir, TEST_CMD);
  if (t.status !== 0) return { ok: false, phase: "test", output: tail((t.stdout || "") + (t.stderr || "")) };
  return { ok: true };
}

// --- prompts -----------------------------------------------------------------

function implementPrompt(name, description) {
  return [
    `Implement the following task in this repository.`,
    `Task: ${name}`,
    ``,
    description,
    ``,
    `Follow .specify/memory/constitution.md. Keep changes scoped to this task.`,
    `Make sure the project builds and tests pass. Do not merge; a human reviews the draft PR.`,
  ].join("\n");
}

function feedbackPrompt(comments) {
  return [
    `A reviewer left feedback on your pull request. Address it on this branch.`,
    ``,
    comments,
    ``,
    `Keep changes minimal and scoped to the feedback. Ensure build and tests still pass.`,
  ].join("\n");
}

function fixPrompt(phase, output) {
  return [
    `The ${phase} step failed. Fix the code so it passes. Output (tail):`,
    "```",
    output,
    "```",
    `Make the minimal change that makes ${phase} pass without breaking other tests.`,
  ].join("\n");
}

// --- git / GitHub helpers (PoC: shell out to git + gh) -----------------------

function prepareWorkspace(repositoryUrl, branch, { create }) {
  const dir = mkdtempSync(`${tmpdir()}/agent-`);
  if (create) {
    git(dir, ["clone", "--depth", "1", repositoryUrl, "."]);
    git(dir, ["checkout", "-b", branch]);
  } else {
    // Existing branch (feedback): clone just that branch.
    git(dir, ["clone", "--depth", "1", "--branch", branch, repositoryUrl, "."]);
  }
  return dir;
}

function commitAndPush(workdir, branch, message) {
  if (!git(workdir, ["status", "--porcelain"]).stdout.trim()) return false;
  git(workdir, ["add", "-A"]);
  git(workdir, ["commit", "-m", message]);
  git(workdir, ["push", "-u", "origin", branch]);
  return true;
}

function openDraftPullRequest(workdir, title, taskId, review, budget) {
  const body =
    `Automated draft for task ${taskId}. Build + tests are green.\n\n` +
    `Agent cost: $${budget.spent.toFixed(2)}\n\n## Reviewer (LLM-as-judge)\n${review}`;
  // gh uses GITHUB_TOKEN from the environment. --draft is the guardrail.
  spawnSync("gh", ["pr", "create", "--draft", "--title", `[agent] ${title}`, "--body", body], {
    cwd: workdir,
    stdio: "inherit",
  });
}

function git(cwd, args) {
  const r = spawnSync("git", args, { cwd, encoding: "utf8" });
  if (r.status !== 0) throw new Error(`git ${args.join(" ")} failed: ${r.stderr}`);
  return r;
}

function run(cwd, cmd) {
  return spawnSync(cmd[0], cmd.slice(1), { cwd, encoding: "utf8" });
}

function tail(s, n = 4000) {
  return s && s.length > n ? s.slice(-n) : s || "";
}

app.listen(PORT, () => console.log(`agent-sidecar listening on :${PORT}`));
