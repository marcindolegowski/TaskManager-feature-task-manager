<!-- SPECKIT START -->
For additional context about technologies to be used, project structure,
shell commands, and other important information, read the current plan
<!-- SPECKIT END -->

# TaskManager — project context

.NET 8 task-management service. The binding rules live in
`.specify/memory/constitution.md` (read it first). The agent deployment strategy
is in `docs/cli-deployment-strategy.md`.

## Architecture
- **CQRS** via `Accessory.Builder.*`: writes through `ICommandDispatcher`, reads
  through `IQueryDispatcher`. Controllers only dispatch.
- **Vertical slices** under `TaskManager.Application/<Feature>/`
  (`Commands/`, `Queries/`, `Events/`, `DTO/`). Example: `Task/`.
- **Domain** in `TaskManager.Core/Domain/`: aggregates own their invariants
  (`Task` with `Status` NotStarted → InProgress → Completed via
  `MoveToNextStatus`); rule breaks throw `BrokenBusinessRuleException`.
- **Messaging**: domain + integration events over RabbitMQ / Service Bus,
  scheduled through the **Outbox** (transactional, exactly-once).
- **Migrations**: DbUp in `TaskManager.DatabaseMigration`.
- **Deploy**: GitHub Actions → ACR → Azure Container Apps.

## Spec-Driven workflow (gated by complexity)
- Simple change → implement directly (no full spec).
- Complex change → `/speckit-specify` → `/speckit-clarify` (if ambiguous) →
  `/speckit-plan` → `/speckit-tasks` → `/speckit-implement`, human review between
  each step.

## Build & test
- `dotnet build TaskManager.Server.sln`
- Run tests before merge; new non-trivial behaviour requires tests.
