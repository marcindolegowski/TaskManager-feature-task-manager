# agent-sidecar

Out-of-process **Claude Agent SDK** service that turns a TaskManager task into a
**draft pull request**. This is the PoC for the strategy in
[`../docs/cli-deployment-strategy.md`](../docs/cli-deployment-strategy.md): the
.NET API orchestrates, the agent runs here, the output is a reviewable draft PR.

## Flow

```
TaskManager API
  └─ RequestTaskImplementationCommand
       └─ publishes TaskImplementationRequested (Outbox → message bus)
            └─ TaskImplementationRequestedHandler → HttpAgentSidecarClient
                 └─ POST /run  ──▶  this sidecar
                       ├─ clone repo, checkout task/{id}
                       ├─ Claude Agent SDK query() with scoped tools   (implement)
                       ├─ build + test gate, fix on failure (FR6)      ◀─ loop
                       ├─ reviewer / LLM-as-judge confidence note (FR7)
                       └─ git push + gh pr create --draft  (green only)

PR review comment  ──(GitHub webhook)──▶  POST /feedback {taskId, branch, comments}
                       └─ re-run pipeline on the existing branch (FR8) → push + gh pr comment
```

## Endpoints

- `POST /run` — `{ taskId, name, description, repositoryUrl }`: implement + open draft PR.
- `POST /feedback` — `{ taskId, repositoryUrl, branch, comments }`: address review
  feedback on the existing branch. Wire a GitHub `pull_request_review_comment` /
  `issue_comment` webhook to this (directly or via a small relay).

## Configuration

| Env var | Default | Purpose |
|---|---|---|
| `ANTHROPIC_API_KEY` | — | Claude Agent SDK auth |
| `GITHUB_TOKEN` | — | `gh` push + draft PR |
| `PORT` | `8787` | sidecar port |
| `BUILD_TARGET` | `TaskManager.Server.sln` | build/test target |
| `BUILD_CMD` / `TEST_CMD` | `dotnet build/test …` | override per repo |
| `MAX_FIX_ATTEMPTS` | `3` | FR6/FR8 fix iterations |
| `COST_CAP_USD` | `2.0` | FR8 per-task cost ceiling |
| `CANDIDATES` | `1` | FR10 best-of-N (≤4); also per-request `candidates` |
| `TASKMANAGER_API_URL` | `http://localhost:5000` | report opened PR back to `/api/agent/result` |

After opening the PR the sidecar POSTs `{taskId, prUrl, costUsd}` to the .NET API's
`/api/agent/result`, which records the `AgentRun` and advances the task to `InProgress`.
A GitHub `pull_request` (merged) webhook to `/api/agent/webhook` then completes the task.

## Run

```bash
cp .env.example .env   # fill in ANTHROPIC_API_KEY and GITHUB_TOKEN
npm install
npm start              # listens on :8787
```

Point the API at it with `AGENT_SIDECAR_URL=http://localhost:8787` (see
`TaskManager.Infrastructure/Extensions.cs`).

Trigger end-to-end:

```bash
curl -X POST http://localhost:5000/api/task/implement \
  -H "Content-Type: application/json" \
  -d '{ "name": "existing-task-name", "repositoryUrl": "https://github.com/org/repo.git" }'
```

## Guardrails (enforced here)

- **Draft PR only**, never auto-merge.
- Work happens on a `task/{id}` branch; never pushes to protected branches.
- The agent's tools are an explicit allowlist (`Read, Grep, Glob, Edit, Write, Bash`).
- Only trusted callers should reach the API's `implement` endpoint — untrusted
  input must not be fed into an autonomous run (prompt-injection risk).

## PoC limitations / TODO

- `@anthropic-ai/claude-agent-sdk` version in `package.json` is a placeholder —
  pin to the latest published version on `npm install`.
- Per-task cost is logged to stdout; wire it into the OpenTelemetry accessory.
- `prepareWorkspace` does a shallow clone with the ambient `GITHUB_TOKEN`; for
  production move execution to an ephemeral, network-scoped sandbox (e.g. ACA Job).
- No PR-merged webhook yet (next slice: → `Task.MoveToNextStatus()`).
