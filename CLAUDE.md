# CLAUDE.md — StoreOps Harness Orchestrator

This file is the root orchestrator for the StoreOps development harness. Claude Code
reads it automatically when launched in this repository. It defines how a developer
starts a harness run, how the four agents hand off work, how verdicts are routed, and
what happens when things go wrong.

## 1. Entry Prompt Format

A developer starts a harness run with a single message addressed to the Planner:

```
@planner <feature description in plain retail-operations language>
```

Example (the demonstration run in this repository):

```
@planner Add shift handover bulk update to activities — PATCH /api/activities/bulk-status,
allowing outgoing shift staff to mark multiple activities DONE or BLOCKED in one request,
with partial failure handling and an audit entry per updated activity.
```

No other invocation format is supported. The Planner is always the entry point; the
Generator and Evaluator are never invoked directly by a developer mid-run.

## 2. Agents

| Agent | File | Reads | Produces |
|---|---|---|---|
| Planner | `.harness/agents/planner.agent.md` | `app-context`, `architecture-principles`, `sprint-decomposition` skills | `spec.md`, `sprint-N-contract.md` |
| Generator | `.harness/agents/generator.agent.md` | `app-context`, `architecture-principles`, `coding-conventions`, `how-to-test` skills | code in `src/`, `generator-summary.md` |
| Evaluator | `.harness/agents/evaluator.agent.md` | `architecture-principles`, `how-to-review`, `evaluation-criteria` skills | `evaluator-feedback.md` |
| Monitor | `.harness/agents/monitor.agent.md` | `app-context`, `evaluator-feedback.md`, `generator-summary.md` | `run-log.md` (archived to `.harness/reviews/`) |

Each agent reads only the skill files listed in its own `.agent.md` file. This keeps
each invocation's context window scoped to what that agent actually needs — the
Generator never loads Evaluator-specific grading criteria, and the Evaluator never
loads Generator coding-convention prose it doesn't need to check against directly
(it checks *output*, not *conventions text*).

## 3. Orchestration Sequence

```
Developer: @planner <feature prompt>
    │
    ▼
Planner reads spec skills → writes .harness/output/spec.md
    (spec.md ends with "STATUS: AWAITING APPROVAL")
    │
    ▼
Developer reviews spec.md, types: APPROVED
    │
    ▼
┌─── Generator/Evaluator loop (autonomous, per sprint) ──────────────┐
│                                                                     │
│  Generator implements sprint-N-contract.md                         │
│      → writes code in src/, .harness/output/generator-summary.md   │
│              │                                                     │
│              ▼                                                     │
│  Evaluator reviews Generator output against hard gates + checklist │
│      → writes .harness/output/evaluator-feedback.md                │
│              │                                                     │
│              ▼                                                     │
│  CLAUDE.md reads the verdict in evaluator-feedback.md:              │
│      PASS            → advance to next sprint contract             │
│      CONDITIONAL PASS → advance, but log the caveat in run-log.md  │
│      FAIL             → send evaluator-feedback.md back to         │
│                          Generator as context, retry                │
│      (max 3 iterations per sprint — see Section 4)                 │
│                                                                     │
└─────────────────────────────────────────────────────────────────┘
    │
    ▼
Monitor reads the completed sprint's evaluator-feedback.md +
generator-summary.md → writes run-log.md, archives all three files
to .harness/reviews/sprint-N-*.md
    │
    ▼
All sprints PASS → harness run complete
```

The developer's only active steps are (1) sending the initial `@planner` prompt and
(2) typing `APPROVED` after reviewing `spec.md`. Everything from Generator through
Monitor runs without further manual triggering.

## 4. Iteration Limit and Escalation

Each sprint gets a maximum of **3 Generator→Evaluator iterations**. If the third
Evaluator verdict is still FAIL, the orchestrator stops the loop and writes
`.harness/output/escalation.md` containing:

- The sprint ID and its contract
- The iteration count (3)
- The specific blocking issue(s) from the final `evaluator-feedback.md` (hard gate
  name + file/line reference)
- A recommendation: does this look like a spec ambiguity (route back to Planner) or
  a Generator implementation gap (needs a human to pair on the fix)?

The Monitor still runs after an escalation — it records the escalation flag and the
blocking issue in `run-log.md` so the pattern is visible in the audit trail even
though no code was accepted.

## 5. Context Scoping Strategy

To prevent context window degradation across a multi-sprint run:

- Each agent invocation starts from a **fresh context window** — the Generator does
  not carry forward its own prior sprint's full conversation, only the current
  `sprint-N-contract.md` plus its own skill files.
- Cross-agent context passes exclusively through the handoff files in
  `.harness/output/` (`spec.md`, `sprint-N-contract.md`, `generator-summary.md`,
  `evaluator-feedback.md`) — never through an implicit shared conversation history.
- On a FAIL retry, the Generator's next invocation receives only the specific
  `evaluator-feedback.md` from the failed attempt, not the full history of all prior
  attempts in that sprint. This keeps retry prompts bounded regardless of how many
  sprints have already run.
- `.harness/output/` is gitignored and treated as scratch space for the *current*
  sprint; `.harness/reviews/` is the permanent, committed record.

## 6. CI/CD Relationship

The Evaluator's automated checks (`dotnet build && dotnet test`, StyleCop, nullable
warnings) **precede** the existing CI pipeline gate, not replace it. The harness is a
pre-commit / pre-PR governance layer: nothing reaches `main` without passing the same
`dotnet build && dotnet test` command the CI pipeline also runs, so the harness and CI
share one source of truth for "green" rather than defining two different bars. The
harness adds LLM-assessed architectural checks (module boundaries, event-bus-only
side effects, error contract) on top of what a standard CI pipeline typically checks
mechanically. `.harness/` is kept separate from `.github/` (see Section 7) specifically
so the harness's working files never get mistaken for pipeline configuration.

## 7. Repository Note

`.harness/` is used instead of `.github/` to keep harness files separate from CI/CD
configuration. This is a deliberate structural choice for this repository, not a
difference from the reference harness pattern.
