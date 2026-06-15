import { spawnSync } from "node:child_process";
import { mkdtempSync } from "node:fs";
import { tmpdir } from "node:os";
import express from "express";
import { query } from "@anthropic-ai/claude-agent-sdk";

// TaskManager agent sidecar (PoC).
//
// Pipeline: task -> implement -> build+test gate (FR6) -> reviewer / LLM-as-judge
// (FR7) -> DRAFT PR. The .NET API never runs the agent in-process; this service does.
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

app.post("/run", (req, res) => {
  const { taskId, name, description, repositoryUrl } = req.body ?? {};
  if (!taskId || !name || !description || !repositoryUrl) {
    return res
      .status(400)
      .json({ error: "taskId, name, description and repositoryUrl are required" });
  }
  // Runs take minutes — acknowledge now, work in the background.
  res.status(202).json({ status: "accepted", taskId });
  runAgent({ taskId, name, description, repositoryUrl }).catch((err) =>
    console.error(`[task ${taskId}] agent run failed:`, err),
  );
});

async function runAgent({ taskId, name, description, repositoryUrl }) {
  const branch = `task/${taskId}`;
  const workdir = prepareWorkspace(repositoryUrl, branch);
  const budget = { spent: 0, cap: COST_CAP_USD };

  // 1. Implement the task.
  await runSession(taskId, workdir, budget, implementPrompt(name, description));

  // 2. FR6: build + test gate. Loop fixes on failure (rich output as feedback).
  let green = false;
  for (let attempt = 0; attempt <= MAX_FIX_ATTEMPTS; attempt++) {
    const check = buildAndTest(workdir);
    if (check.ok) {
      green = true;
      break;
    }
    if (attempt === MAX_FIX_ATTEMPTS || budget.spent >= budget.cap) break;
    console.log(`[task ${taskId}] ${check.phase} failed (attempt ${attempt + 1}); asking agent to fix`);
    await runSession(taskId, workdir, budget, fixPrompt(check.phase, check.output));
  }

  // FR6: open the PR only when green.
  if (!green) {
    console.warn(
      `[task ${taskId}] not green after ${MAX_FIX_ATTEMPTS} attempts ` +
        `($${budget.spent.toFixed(2)} spent); NOT opening PR`,
    );
    return;
  }

  // 3. FR7: independent reviewer / LLM-as-judge.
  const review = await reviewPatch(taskId, workdir, budget, name, description);

  // 4. Open a draft PR and attach the review + cost.
  openDraftPullRequest(workdir, branch, name, taskId, review, budget);
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

// FR7: read the diff and produce a confidence verdict (no edits — read-only tools).
async function reviewPatch(taskId, cwd, budget, name, description) {
  const diff = git(cwd, ["diff", "HEAD"]).stdout;
  let verdict = "";
  for await (const message of query({
    prompt:
      `Act as an independent reviewer (LLM-as-judge) for this change.\n` +
      `Task: ${name}\n${description}\n\nDiff:\n${tail(diff, 12000)}\n\n` +
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

function fixPrompt(phase, output) {
  return [
    `The ${phase} step failed. Fix the code so it passes. Output (tail):`,
    "```",
    output,
    "```",
    `Make the minimal change that makes ${phase} pass without breaking other tests.`,
  ].join("\n");
}

// --- Git/GitHub helpers (PoC: shell out to git + gh) -------------------------

function prepareWorkspace(repositoryUrl, branch) {
  const dir = mkdtempSync(`${tmpdir()}/agent-`);
  git(dir, ["clone", "--depth", "1", repositoryUrl, "."]);
  git(dir, ["checkout", "-b", branch]);
  return dir;
}

function openDraftPullRequest(workdir, branch, title, taskId, review, budget) {
  const status = git(workdir, ["status", "--porcelain"]).stdout.trim();
  if (!status) {
    console.log(`[task ${taskId}] no changes produced; skipping PR`);
    return;
  }
  git(workdir, ["add", "-A"]);
  git(workdir, ["commit", "-m", `Agent implementation for: ${title}`]);
  git(workdir, ["push", "-u", "origin", branch]);
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
