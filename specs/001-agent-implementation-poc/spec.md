# Feature Spec: Agent Implementation PoC (task → draft PR)

**Feature ID**: 001-agent-implementation-poc
**Status**: Draft (PoC) · **Created**: 2026-06-14
**Constitution**: `.specify/memory/constitution.md` · **Strategy**: `docs/cli-deployment-strategy.md`

## Summary

Turn a TaskManager task into a **draft pull request** by dispatching the task's
`Name` + `Description` to an out-of-process **Claude Agent SDK** sidecar. This is
the smallest end-to-end slice of the agent deployment strategy: TaskManager
orchestrates, the agent runs elsewhere, output is a reviewable draft PR.

## User scenario

1. A trusted operator calls `POST /api/task/implement { name, repositoryUrl }`.
2. The command handler loads the task and publishes a `TaskImplementationRequested`
   integration event (through the existing Outbox/message bus).
3. A subscriber hands the event to the agent sidecar.
4. The sidecar clones the repo, works on a `task/{id}` branch, runs the agent, and
   opens a **draft PR**.
5. A human reviews the PR; on merge, the task advances to `Completed` (future slice).

## Functional requirements

- **FR1**: Publishing the trigger is transactional via the Outbox (exactly-once;
  no run if the transaction rolls back).
- **FR2**: The event carries `TaskId`, `Name`, `Description`, `RepositoryUrl`.
- **FR3**: The sidecar exposes `POST /run` accepting that payload and acknowledges
  immediately (runs are minutes-long, executed in the background).
- **FR4**: The agent runs with a scoped tool allowlist and produces a **draft PR
  only** — never auto-merge, never push to protected branches.
- **FR5**: Untrusted input must not reach the agent (the `implement` endpoint is for
  trusted callers; the public `POST /task` is not wired to the agent).

### Evidence-based requirements

Backed by program-repair / SWE-agent research (see `docs/cli-deployment-strategy.md`
§8 for citations). These raise patch quality and are the adoption order for the PoC.

- **FR6 (test-execution gate)**: the agent loops `dotnet build` + tests, reads
  failures (with full traces, not just the failing line), and opens the PR **only
  when green**. Highest-ROI lever in the literature.
- **FR7 (reviewer / LLM-as-judge)**: before the human, an automated review checks
  the patch against the spec and posts a **confidence score + explanation** on the PR.
- **FR8 (feedback loop with memory + rollback)**: PR comments re-trigger a resumed
  session; keep attempt history, require each iteration to strictly improve, and
  **cap iterations and cost** per task.
- **FR9 (intent inference)**: derive and verify a spec from the task instead of
  passing the one-line `Description` verbatim (`/speckit-specify` → `/speckit-clarify`).
- **FR10 (best-of-N, gated by complexity)**: for complex tasks only, sample N
  candidate patches and select by green tests / highest judge confidence.

## Success criteria

- A task name + repo URL results in a draft PR on a `task/{id}` branch.
- No agent code runs inside the API process.
- The PR is opened only after `dotnet build` + tests pass (FR6).
- An automated reviewer confidence note is present on the PR (FR7).
- Token usage/cost is logged per `TaskId` (observability principle).
- Realistic expectation: the agent produces a reviewable first draft, not a
  guaranteed-mergeable change (~25–31% land rate in comparable deployments).

## Out of scope (this PoC)

- PR-merged webhook → `Task.MoveToNextStatus()` (next slice).
- RAG / code-search MCP layer (agentic search is enough for this repo).
- ACA Job / KEDA trigger transport (start with a direct call; move to the bus next).
- Multi-model routing / self-hosting.

## Key entities / contracts

- `TaskImplementationRequested` (integration event)
- `IAgentSidecarClient` (application port) → `HttpAgentSidecarClient` (adapter)
- `RequestTaskImplementationCommand` (+ handler) — trigger
- Sidecar `POST /run` — `{ taskId, name, description, repositoryUrl }`
