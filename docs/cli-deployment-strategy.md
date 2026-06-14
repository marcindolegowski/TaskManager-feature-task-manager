# CLI / Coding-Agent Deployment Strategy

> Status: **Decided (PoC pending)** · Last updated: 2026-06-14
>
> This document records *where and how we run an AI coding agent* in relation to
> TaskManager, the options we evaluated, and why we landed where we did. It is a
> decision record, not a tutorial.

## 1. Problem

We want a "task → implementation" step: a TaskManager task (which already carries
a `Name` + natural-language `Description`) should be turned into working code and a
**pull request**, with an experience that *feels like* Claude Code / Cursor /
Copilot — i.e. real context gathering, retrieval, an agentic edit→test→fix loop,
caching — **not** a single raw model call.

Two hard requirements emerged while evaluating:

1. **Quality / feel** — must match a polished coding agent (Claude Code-grade), not
   a stitched-together batch pipeline.
2. **High volume** — many runs, so cost behaviour matters.

## 2. Decision

**Embed the [Claude Agent SDK](https://code.claude.com/docs/en/agent-sdk/overview)
(the same harness that powers Claude Code) as a sidecar service behind the .NET
TaskManager API.** TaskManager stays the orchestrator; the agent runs out-of-process.

```
TaskManager (.NET, orchestrator)
   │  publishes TaskImplementationRequested via the existing Outbox → message bus
   ▼
Agent sidecar (Node/TypeScript or Python, Claude Agent SDK)
   ├─ context gathering: agentic search + tools (Read/Grep/Glob/Bash)   [SDK harness]
   ├─ RAG (optional, add when repo grows): code-search MCP server        [pluggable]
   ├─ caching: Anthropic prompt caching                                  [model layer]
   └─ opens a DRAFT pull request
   ▼
GitHub PR  →  (merge webhook)  →  Task.MoveToNextStatus() → Completed
```

The agent **never runs in the API process** (security, cost, latency, unpredictable
runtime). The app's existing CQRS + domain/integration events + Outbox give us a
reliable, transactional trigger (exactly-once, no double runs).

### Why Claude Agent SDK specifically
- It is the **embeddable form of Claude Code** — same subagents, sessions, MCP, tool
  use and agent loop — so we get the Claude Code feel as *programmable infrastructure*,
  not an interactive CLI we have to shell out to.
- The fact that it is TypeScript/Python (no official C# Agent SDK) is a non-issue with
  the sidecar pattern: the .NET app talks to a small Node/Python service over HTTP/queue.
- We consciously accept **Anthropic-only models** for now: the team prefers staying on
  Claude Code and does not currently have a Cursor subscription.

## 3. Options considered (and why rejected)

| Option | Verdict | Reason |
|---|---|---|
| **Embed agent in the .NET API process** | ❌ | No C# Agent SDK; running an agent with FS write + shell inside the request-handling web server is a security/cost/latency hazard. |
| **Raw model call (Messages API) + our own loop** | ❌ | Loses the techniques that create the feel (context gathering, RAG, caching, agentic loop). "Not a raw model." |
| **OSS agent (OpenHands/Cline) + LiteLLM + MCP RAG** | ❌ for this goal | Vendor-neutral and cheap-at-scale, **but** cannot reproduce the polished low-latency feel (that lives in proprietary harnesses/models). Good batch PR pipeline, wrong product. Kept as a future cost-down lever via self-hosted open models. |
| **Claude Code Routines / Claude Code on the web** | ❌ | Managed and low-infra, but tied to an individual claude.ai subscription and Anthropic-only; not a fit for an embedded, high-volume product surface. |
| **Cursor SDK** | ⚠️ strong alternative | Best "feel + multi-model + white-label" match (used by Notion/Rippling/Faire). Deferred only because we have no Cursor subscription right now. Revisit if multi-model becomes a hard requirement. |
| **Managed Agents API (self-hosted sandbox)** | ⚠️ later | Good for scale + data-residency in our Azure env; more integration to build. Revisit at productisation. |
| **GitHub Actions agent mode / ACA Job (KEDA on RabbitMQ)** | ✅ as trigger transport | Not the engine, but a clean way to *run* the sidecar in our existing Azure/GitHub stack. |

## 4. The trilemma (why we can't have everything)

You get **two of three**: *Claude-Code-grade feel*, *vendor-neutral*, *cheap at high
volume*.

- The "feel" lives in proprietary models + harness you **cannot self-host**.
- A single autonomous task burns **~400K–2M input tokens**; input ≫ output (20–25×).
  At high volume, pay-per-token frontier API can exceed a flat subscription.
- Therefore: with the feel prioritised, **cost is managed, not eliminated** — via
  prompt caching, routing cheap models to simple tasks, and treating agent spend as a
  product cost. Self-hosting open models is the only true cost-down, and it sacrifices
  feel — kept as an explicit future lever, not the default.

**Our priority order: feel > vendor-neutrality > cost.**

## 5. Guardrails (non-negotiable)

- **Trusted task sources only.** `Description` becomes an autonomous agent prompt with
  repo + shell access → prompt-injection / supply-chain risk. The public `POST /task`
  endpoint must not feed untrusted input straight into a run.
- **Draft PR always, never auto-merge.** Human review gate.
- **Branch-scoped pushes** (`claude/*`/`task/{id}` branches only).
- **Least privilege**: scoped tools/permissions, one repo per run, ephemeral sandbox.
- **Observability**: emit the run's token usage / cost as OpenTelemetry spans tied to
  `TaskId` (we already have the OpenTelemetry accessory) for per-task cost tracking.

## 6. PoC plan (smallest end-to-end slice)

1. Stand up a minimal **Node sidecar** using the Claude Agent SDK that accepts
   `{ taskId, name, description, repo }` and runs one agent session.
2. Wire the trigger: a handler on `TaskImplementationRequested` (via Outbox) calls the
   sidecar. For the very first slice, a direct HTTP call is fine; move to the message
   bus / an ACA Job (KEDA on RabbitMQ) once it works.
3. Agent clones the repo, works on a `task/{id}` branch, opens a **draft PR**.
4. Write `PrUrl` / `AgentRunId` back onto the `Task`; status → `InProgress`.
5. On PR-merged webhook → `Task.MoveToNextStatus()` → `Completed`.
6. Add prompt caching; measure tokens/cost per run via OpenTelemetry before scaling.

Defer until needed: the code-search **MCP RAG** layer (agentic search is enough for a
~300-file repo; RAG pays off on large monorepos) and any multi-model / self-host work.

## 7. Open questions

- Auth & billing for the sidecar: Anthropic **API key** (pay-per-token, service
  identity) vs a Claude subscription login — decide before high-volume rollout.
- Where the sidecar runs: ACA Job (KEDA on our RabbitMQ) vs a long-lived container.
- Revisit Cursor SDK (multi-model) if vendor-neutrality becomes a hard requirement.

## 8. Evidence-based improvements (research-backed)

How to make the `task → PR` process more reliable, ranked by strength of evidence.
Each lever maps to a concrete change in the sidecar / SDD flow. References are
peer-reviewed / arXiv program-repair and SWE-agent literature.

1. **Execution/test feedback as the core loop (highest ROI).** Reading test/run
   output and iterating beats one-shot generation: Self-Debugging adds up to +12%
   and matches 10× sampling efficiency [3]; iterative repair on test signal alone
   gives +65% Pass@1 in TraceCoder [4]; production ReAct + tests + static analysis
   solves at ~11.8 feedback iterations [8]. → The sidecar MUST loop
   `dotnet build` + tests, read failures, and only open the PR when green.

2. **Feed runtime evidence, not just stack traces.** Intermediate runtime state
   beats surface logs: DebugRepair +26% [5], interactive-debugger dynamic analysis
   +5–60% [1], trace-driven analysis [4]. → On red tests, pass full traces/asserts
   to the agent, not just the failure line.

3. **Structure- and test-aware fault localization (not just grep).**
   AutoCodeRover uses AST-level code search + spectrum-based localization → 19% on
   SWE-bench-lite at ~$0.43 / ~4 min per issue [7]; contrastive failing/passing
   test pairs sharpen root-cause [9]. → Enrich context with compiler errors + test
   stack traces as a localization signal.

4. **Reviewer agent / LLM-as-judge before the human.** Production APR pairs the
   agent with an LLM-as-Judge then human review [8]; SpecRover's reviewer emits a
   confidence measure → +50% over its baseline at $0.65/issue [10]. → Add an
   automated patch-vs-spec review with a confidence score, posted on the PR (our
   SDD `/speckit-analyze`).

5. **Specification inference + verification (the science behind our SDD layer).**
   SpecRover shows inferring and verifying intent yields +50% [10]. → Don't pass the
   one-line `Description` verbatim; derive and verify a spec (`specify` / `clarify`).

6. **Feedback loop with memory + rollback.** Keep a memory of failed attempts and
   require each iteration to strictly improve (TraceCoder HLLM + rollback [4]); rank
   candidates and balance repair-vs-regenerate (SEIDR [6]). → The PR-comment loop
   keeps attempt history, enforces improvement, and caps iterations/cost.

7. **Best-of-N, gated by complexity.** Sampling multiple patches and selecting by
   tests/confidence raises success at token cost [3][6]. → Hard tasks only: N
   candidates → pick the one with green tests / highest judge confidence.

8. **Stage the work (Explore → Plan → Implement → Verify) + repo context files.**
   Staging reduces errors; `CLAUDE.md` / constitution keep output in-bounds; treat
   agent output like a junior dev's — verify everything (practitioner consensus).

**Reality check.** In a real large-codebase deployment only ~25–31% of generated
fixes landed after review; the rest were valued as "good starting points" [8]. The
agent is an accelerator / first draft, not an autopilot — which is exactly why we
keep draft-PR-only + a human gate. The upside: well-scaffolded agents are cheap and
fast per task ($0.43–0.65, minutes) [7][10]; the cost concern is volume, not a run.

**Adoption order for the PoC:** (1) test-execution gate, (2) reviewer/LLM-as-judge
step, (3) memory+rollback feedback loop, (4) structure/test-aware localization +
best-of-N for hard tasks, (5) enforce `specify`/`clarify` as intent inference.

### References
- [1] InspectCoder — https://consensus.app/papers/details/93e37b0f3d20500d99d901fbff7396f7/
- [3] Teaching LLMs to Self-Debug — https://consensus.app/papers/details/7cfefb6d86e553ae8a92e0881d16891a/
- [4] TraceCoder — https://consensus.app/papers/details/e689ae2b21d758d9a8d08e40aaade104/
- [5] DebugRepair — https://consensus.app/papers/details/826a10a5c0ee5bea90be0426060b216b/
- [6] SEIDR (Iterative Multi-Agent Debugging) — https://consensus.app/papers/details/85ec6e3d721b589d84a872a8825d3bc1/
- [7] AutoCodeRover — https://consensus.app/papers/details/fcabd95011765eea90d57a6b2f3af957/
- [8] Agentic Program Repair from Test Failures at Scale — https://consensus.app/papers/details/7bafa39bc5725a9f860cc1eb2a6acc2e/
- [9] ContrastRepair — https://consensus.app/papers/details/e1a1c2d702935c3cacab2dc349a02d7f/
- [10] SpecRover — https://consensus.app/papers/details/ccf795fd03c454028066a85b371be803/
