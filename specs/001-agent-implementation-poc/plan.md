# Implementation Plan: Agent Implementation PoC (task → draft PR)

**Feature ID**: 001-agent-implementation-poc · **Spec**: [`spec.md`](./spec.md)
**Constitution**: `.specify/memory/constitution.md` · **Strategy**: `docs/cli-deployment-strategy.md`

This plan turns the spec into a technical approach. It assumes the chosen engine
(Claude Agent SDK, out-of-process sidecar) and folds in the evidence-based
improvements (spec §FR6–FR10, strategy §8).

## Architecture

```
TaskManager API (.NET)                      agent-sidecar (Node, Claude Agent SDK)
  POST /api/task/implement                    POST /run  ── 202 Accepted
   └ RequestTaskImplementationCommand         └ background pipeline:
      └ publish TaskImplementationRequested      1. clone repo, checkout task/{id}
         (Outbox → message bus)                  2. derive/verify spec  (FR9)
            └ TaskImplementationRequestedHandler 3. plan → implement    (Explore/Plan)
               └ HttpAgentSidecarClient.POST     4. build + test loop    (FR6)  ◀─┐
                                                 5. reviewer / judge     (FR7)    │ iterate
                                                 6. open DRAFT PR + confidence note │ (rollback,
   PR comment webhook ───────────────────────▶  7. resume on feedback   (FR8) ────┘  memory, cap)
   PR merged webhook ─────────▶ Task.MoveToNextStatus() → Completed
```

## Components & status

| Component | State | Notes |
|---|---|---|
| `TaskImplementationRequested` event + handler | ✅ scaffolded | Application/Task/Events |
| `IAgentSidecarClient` + `HttpAgentSidecarClient` | ✅ scaffolded | port/adapter |
| `RequestTaskImplementationCommand` (+ handler) + endpoint | ✅ scaffolded | trigger |
| Sidecar `POST /run` (clone, agent run, draft PR) | ✅ scaffolded | `agent-sidecar/` |
| Build+test gate (FR6) | ✅ in sidecar | loops `dotnet build`+tests, fixes on failure, PR only when green |
| Reviewer / LLM-as-judge (FR7) | ✅ in sidecar | reviews diff, confidence note in PR body |
| Iteration + cost caps (FR8 partial) | ✅ in sidecar | `MAX_FIX_ATTEMPTS`, `COST_CAP_USD` |
| Feedback loop on PR comments (FR8) | ✅ in sidecar | `POST /feedback` re-runs pipeline on existing branch; webhook wiring pending |
| Intent inference: derive spec (FR9) | ✅ in sidecar | grounded spec step before coding; spec in PR body |
| Best-of-N for complex tasks (FR10) | ✅ in sidecar | `candidates` (≤4); pick highest reviewer confidence among green |
| Status + `PrUrl`/`AgentRunId` on `Task` | ⬜ next slice | progress visibility |
| PR-merged webhook → `MoveToNextStatus()` | ⬜ next slice | close the loop |

## Build order (evidence-ranked)

1. **FR6 — test-execution gate.** In the sidecar, after the agent run, run
   `dotnet build TaskManager.Server.sln` + test suite; on failure feed full output
   back into a resumed `query()` and retry (cap N). Open the PR only when green.
2. **FR7 — reviewer step.** Second agent/judge call: review the diff against the
   derived spec, output `{confidence, summary, risks}`; post as a PR comment.
3. **FR8 — feedback loop.** Webhook on `pull_request_review_comment`/`issue_comment`
   → resume the session with comments + attempt history; rollback if a step regresses;
   enforce max iterations and a per-task cost ceiling.
4. **FR9 — intent inference.** Run `/speckit-specify` (+ `/speckit-clarify` when
   ambiguous) to produce the spec the agent implements, instead of the raw `Description`.
5. **FR10 — best-of-N.** For complex tasks, sample N patches; select by green tests
   then judge confidence.

## Cross-cutting

- **Observability**: emit per-`TaskId` token/cost + iteration count + final
  judge confidence as OpenTelemetry spans (constitution §V).
- **Guardrails**: draft PR only; `task/{id}` branches; scoped tools; trusted sources;
  ephemeral sandbox (move execution to an ACA Job for network/file isolation).
- **Cost/latency budget**: target the literature's envelope (~$0.4–0.7, minutes per
  task); enforce caps so a runaway loop cannot exceed it.

## Risks

- Reward hacking: agent games tests. Mitigation: judge reviews intent vs diff (FR7),
  human gate, held-out checks where feasible.
- Documentation drift: avoided by keeping spec/plan ephemeral with the PR branch.
- Land rate ~25–31% (comparable deployments): set expectations — accelerator, not autopilot.
