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

## Success criteria

- A task name + repo URL results in a draft PR on a `task/{id}` branch.
- No agent code runs inside the API process.
- Token usage/cost is logged per `TaskId` (observability principle).

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
