# TaskManager Constitution

<!--
Foundational, agent-binding principles for the TaskManager codebase. Spec Kit
loads this file (`.specify/memory/constitution.md`) as the source of truth that
`/speckit-plan` and `/speckit-implement` must obey. Keep it concrete and testable.
-->

## Core Principles

### I. CQRS + Vertical Slices
Application logic is organised as commands and queries, not anaemic services.
Writes go through `ICommandDispatcher`, reads through `IQueryDispatcher`. Each
feature lives as a vertical slice under `TaskManager.Application/<Feature>/`
(`Commands/`, `Queries/`, `Events/`, `DTO/`) with one handler per command/query.
New behaviour MUST follow this shape — no business logic in controllers
(controllers only dispatch), and no mixing read and write concerns in one handler.

### II. Rich Domain, Explicit Events
Business invariants live in the domain (`TaskManager.Core/Domain/`), not in
handlers. Aggregates (e.g. `Task : IAggregateRoot<TaskId>`) own their state
transitions (`MoveToNextStatus`) and raise domain events via `AddDomainEvent`.
Rule violations throw `BrokenBusinessRuleException` with an explicit rule type.
Never expose public setters to bypass invariants; never duplicate a domain rule
in application code.

### III. Reliable, Asynchronous Messaging (NON-NEGOTIABLE)
Cross-boundary side effects are delivered through integration events on the
message bus, scheduled via the **Outbox** so they commit in the same transaction
as the state change (exactly-once, no lost or duplicated effects). Long-running
or external work (including any AI "implementation" step) MUST be triggered by an
event consumed out-of-process — never executed inline in an API request.

### IV. Quality Gates Before Merge
`dotnet build` and the test suite MUST pass before any change merges. Non-trivial
changes (multi-file, new aggregate, new event contract, schema change) require
tests. Database changes go through DbUp migrations in
`TaskManager.DatabaseMigration` — never hand-edited schema drift.

### V. Observability by Default
Use the OpenTelemetry accessory for structured logging, traces and metrics.
Any agent-driven run MUST emit token usage / cost as telemetry correlated to the
originating `TaskId`, so per-task cost is measurable before scaling.

## Technology Constraints

- **Runtime**: .NET 8. **Persistence**: SQL Server (EF Core + Dapper accessories).
  **Messaging**: RabbitMQ / Azure Service Bus via `Accessory.Builder.MessageBus.*`.
  **Migrations**: DbUp. **Deploy**: GitHub Actions → ACR → Azure Container Apps.
- Reuse the `Accessory.Builder.*` packages (CQRS, MessageBus, Outbox, Redis,
  HttpClient, Logging.OpenTelemetry, Persistence.*) instead of re-implementing
  cross-cutting concerns. A new dependency must be justified against an existing
  accessory.
- The AI coding agent is the **Claude Agent SDK** harness, run as an
  **out-of-process sidecar** behind the API (see `docs/cli-deployment-strategy.md`).
  TaskManager orchestrates; the agent never runs in the web process.

## Agentic Development Workflow

Spec-Driven Development is applied **gated by complexity** — process serves
throughput, not the reverse:

- **Simple change** (single file, formatting, well-understood CRUD): go straight
  to implementation. Do NOT author a full spec — that costs more than it saves.
- **Complex change** (multi-file, architectural, new integration/event contract,
  schema): run the full flow `/speckit-specify → /speckit-clarify (if ambiguous)
  → /speckit-plan → /speckit-tasks → /speckit-implement`, with human review
  between each artifact.

Guardrails for every agent run (whichever path):
- Output is always a **draft pull request**; **never auto-merge**.
- Push only to `claude/*` or `task/{id}` branches; never to protected branches.
- Only **trusted task sources** feed an agent prompt. Untrusted input (e.g. a
  public `POST /task` body) MUST NOT be passed verbatim into an autonomous run
  (prompt-injection / supply-chain risk).
- Least privilege: scoped tools/permissions, one repository per run, ephemeral
  sandbox.
- Spec/plan/tasks artifacts are ephemeral per task and live with the PR branch,
  keeping the spec tied to the change (avoids long-lived documentation drift).

## Governance

This constitution supersedes ad-hoc practice. Every PR and review — human or
agent — MUST verify compliance with the principles above. Deviations require an
explicit, written justification in the PR description. Amendments are made by PR
editing this file, bumping the version below per semantic-versioning rules
(MAJOR: principle removed/redefined; MINOR: principle/section added; PATCH:
clarification).

**Version**: 1.0.0 | **Ratified**: 2026-06-14 | **Last Amended**: 2026-06-14
