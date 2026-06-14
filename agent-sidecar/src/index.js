import { spawnSync } from "node:child_process";
import { mkdtempSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import express from "express";
import { query } from "@anthropic-ai/claude-agent-sdk";

// TaskManager agent sidecar (PoC).
//
// Receives a task from the .NET API and turns it into a DRAFT pull request using
// the Claude Agent SDK. The API never runs the agent in-process — this service does.
// Guardrails (see ../.specify/memory/constitution.md): scoped tools, work on a
// `task/{id}` branch, draft PR only, never auto-merge.

const PORT = process.env.PORT || 8787;
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

  const prompt = [
    `Implement the following task in this repository.`,
    `Task: ${name}`,
    ``,
    description,
    ``,
    `Follow .specify/memory/constitution.md. Keep changes scoped to this task.`,
    `Do not merge; a human will review the draft PR.`,
  ].join("\n");

  let usage;
  for await (const message of query({
    prompt,
    options: {
      cwd: workdir,
      // Autonomous run: auto-apply edits, but tools are explicitly scoped.
      permissionMode: "acceptEdits",
      allowedTools: ["Read", "Grep", "Glob", "Edit", "Write", "Bash"],
      systemPrompt:
        "You are a senior engineer. Obey the repository constitution at " +
        ".specify/memory/constitution.md. Produce a focused, reviewable change.",
    },
  })) {
    if (message.type === "result") usage = message;
    console.log(`[task ${taskId}]`, JSON.stringify(message));
  }

  // Observability principle: surface per-task cost (wire into OpenTelemetry later).
  if (usage?.total_cost_usd != null) {
    console.log(`[task ${taskId}] cost_usd=${usage.total_cost_usd}`);
  }

  openDraftPullRequest(workdir, branch, name, taskId);
}

// --- Git/GitHub helpers (PoC: shell out to git + gh) -------------------------

function prepareWorkspace(repositoryUrl, branch) {
  const dir = mkdtempSync(join(tmpdir(), "agent-"));
  git(dir, ["clone", "--depth", "1", repositoryUrl, "."]);
  git(dir, ["checkout", "-b", branch]);
  return dir;
}

function openDraftPullRequest(workdir, branch, title, taskId) {
  // Nothing to do if the agent made no changes.
  const status = git(workdir, ["status", "--porcelain"]).stdout.trim();
  if (!status) {
    console.log(`[task ${taskId}] no changes produced; skipping PR`);
    return;
  }
  git(workdir, ["add", "-A"]);
  git(workdir, ["commit", "-m", `Agent implementation for: ${title}`]);
  git(workdir, ["push", "-u", "origin", branch]);
  // gh uses GITHUB_TOKEN from the environment. --draft is the guardrail.
  spawnSync(
    "gh",
    ["pr", "create", "--draft", "--title", `[agent] ${title}`, "--body", `Automated draft for task ${taskId}.`],
    { cwd: workdir, stdio: "inherit" },
  );
}

function git(cwd, args) {
  const r = spawnSync("git", args, { cwd, encoding: "utf8" });
  if (r.status !== 0) {
    throw new Error(`git ${args.join(" ")} failed: ${r.stderr}`);
  }
  return r;
}

app.listen(PORT, () => console.log(`agent-sidecar listening on :${PORT}`));
