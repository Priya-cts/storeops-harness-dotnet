# Harness Design Brief

**Feature governed:** Shift handover bulk update — `PATCH /api/activities/bulk-status`
**Stack:** .NET 8 / ASP.NET Core Web API, C# 12
**Client context:** StoreOps API, retail store operations squad (Section 2, programme case study)

---

## Section A — Intent Decomposition

### How the feature was broken into sprint contracts

This feature was scoped as a **single sprint**. The sprint-sizing rule in
`planner.agent.md` is: one sprint = one cohesive Routes → Service → Repository
change plus its tests. The bulk-status feature touches exactly one new Service
method, one new Controller action, one new set of DTOs, and one new cross-module
event contract — all in service of one user-facing capability (an outgoing shift
worker closing out a batch of activities). Splitting it further (e.g. "sprint 1:
the endpoint, sprint 2: the audit event") would have created an artificial seam
where the Evaluator would have to pass code that publishes an event nobody
subscribes to yet, which doesn't actually reduce risk — it just defers the one
review that matters (does the whole thing work together) to a later sprint.

Had the feature instead required, say, both this bulk endpoint *and* a new
SLA-breach escalation path (the second suggested feature in Section 3.4), I would
split those into two sprints, because they exercise genuinely independent
acceptance criteria against different trigger conditions (a direct staff action vs.
a time-based background check) and gate independently — a Generator failure on one
shouldn't block the other from shipping.

### How acceptance criteria were structured

Every criterion in `sprint-1-contract.md` follows GIVEN/WHEN/THEN and is written so
that "pass" or "fail" can be determined by a specific test, not a judgment call. I
treated "testable" as the actual constraint, not just the GIVEN/WHEN/THEN format —
a criterion like "the feature should feel fast" would technically fit the template
but isn't testable; "the response is 200 OK with each id listed under Updated" is.

### Example sprint contract entry, in full

> **AC2**
> GIVEN a request body where some `activityIds` exist and some do not
> WHEN `PATCH /api/activities/bulk-status` is called
> THEN the existing activities are updated and listed under `Updated`, the
> non-existent ids are listed under `Failed` with reason `"Activity not found."`,
> and the response is still `200 OK` (partial failure does not abort the whole
> request).

This is the criterion I consider the actual design decision of the sprint (see
Section D) — it's also the one where "testable" mattered most, because the naive
alternative ("throw `NotFoundError` if any id is missing") is just as easy to write
and just as easy to test, but is the wrong behavior for a bulk operation. Writing
the criterion explicitly, before the Generator touched any code, is what prevented
that wrong-but-plausible implementation from ever being on the table.

---

## Section B — Governance Framework

### Skill file strategy

Six skill files, split by who reads them and why:

- **`app-context`** and **`architecture-principles`** are read by all three
  active agents (Planner, Generator, Evaluator). They're the shared vocabulary and
  shared rulebook — splitting rules across per-agent files would let the
  Generator's understanding of "what counts as a module boundary violation" drift
  from the Evaluator's understanding of the same rule, which defeats the purpose of
  having a rule at all.
- **`coding-conventions`** and **`how-to-test`** are Generator-only. They're
  *how to comply*, phrased as instructions (naming, DI lifetimes, test file
  layout). The Evaluator doesn't need "how to write a controller" — it needs "how
  to check whether a controller was written correctly," which is a different skill.
- **`how-to-review`** and **`evaluation-criteria`** are Evaluator-only for the
  same reason in reverse: they're a review procedure and a scoring rubric, not
  something the Generator should be optimizing against directly (a Generator that
  read the exact scoring weights might over-fit effort to the highest-weighted
  dimension rather than just building the feature correctly).

Every rule in every skill file is anchored to something specific to StoreOps, not a
generic principle: `architecture-principles/SKILL.md` states each rule *and* which
of the client's four reported failure modes (Section 2 of the case study) it
prevents; `coding-conventions/SKILL.md` states the actual DI lifetime StoreOps
repositories must use and why getting it wrong is a silent bug specifically in this
codebase's in-memory-storage design.

### How `.harness/reviews/` functions as a governance audit trail

Every sprint's `spec.md`, `sprint-N-contract.md`, `generator-summary.md`,
`evaluator-feedback.md`, and `run-log.md` are copied (not moved — the working
copies in `.harness/output/` are gitignored and reset per sprint) into
`.harness/reviews/` and committed. This gives three things a solution architect
or engineering-standards reviewer would want, none of which exist if you only look
at the final `git diff`:

1. **What was asked for** (`spec.md`, `sprint-N-contract.md`) vs. **what was
   verified** (`evaluator-feedback.md`) — so a reviewer can check the contract was
   actually satisfied, not just that *some* code was written.
2. **A verdict with reasoning**, not just a merged PR — `evaluator-feedback.md`
   names specific files and lines, so "why was this accepted" is answered without
   re-deriving it.
3. **A trend surface**. `monitor.agent.md` explicitly asks the Monitor to compare
   each new sprint's hard-gate failure pattern against prior `run-log.md` entries
   in the same repository. If, three sprints from now, Dimension 2 (module
   boundary) keeps failing specifically in the Reports module, that pattern is
   visible in `.harness/reviews/` without anyone having to remember it — it's the
   kind of thing that currently lives only in a senior engineer's memory of past
   PR reviews, which doesn't scale past that one engineer.

### One skill file rule, traced to a StoreOps architecture decision

**Rule**: any side effect that crosses a module boundary must be raised via
`IEventBus.Publish(...)` with a typed record from `Shared/Events`, never by one
module directly calling another module's service for a *write*.

**What breaks without it**: this is failure mode #4 from the client's prior
experiment — AI-generated code wrote directly into a sibling module's repository to
raise a notification. In this sprint specifically, without the rule,
`ActivityService.BulkUpdateStatusAsync` would have been free to inject
`IAlertService` directly and call `CreateNotification(...)` on it. That's less
code, and it would pass every unit test I wrote. What it breaks: Activities can no
longer be built, tested, or deployed independently of Alerts (a change to
`Notification`'s constructor now requires touching Activities' code too, even
though nothing about bulk status updates conceptually changed); and it becomes
much harder to later swap Alerts' delivery mechanism (e.g. add email) without
touching every module that triggers a notification, because the coupling point is
scattered across every caller instead of centralized at the bus.

---

## Section C — Non-Determinism Strategy

### Evaluation dimensions, weights, and why (for StoreOps specifically)

The six dimensions in `evaluation-criteria/SKILL.md` are not a generic code-review
checklist — four of the six map directly onto the four failure modes the client's
standards team reported (Section 2 of the case study): module boundary (20%),
event bus (15%), error contract (15%), and test quality's "asserts business rules,
not just status codes" half (part of the 10%). The remaining two — build/test
correctness (30%, the largest single weight) and layer separation (10%) — exist
because a harness that *only* checked the four reported failure modes would still
pass code that doesn't compile, or code that technically avoids all four named
failure modes while putting business logic in a Controller (a fifth failure mode
nobody had reported yet, precisely because until a harness like this exists,
nobody's evaluator was looking for it specifically).

Build/test correctness gets the largest weight (30%) deliberately: it is the one
dimension that is 100% automated (an exit code), so it is the cheapest possible
signal, and it is also a hard precondition for every other dimension meaning
anything (there is no point hand-reviewing module boundaries in code that doesn't
even compile).

### Hard gate conditions and why each cannot be a soft check

- **Build/test hard gate**: a build error or a failing test is binary and
  automatable — making it a "soft" weighted-score contributor instead would mean a
  sprint with beautifully architected but *non-compiling* code could still average
  out to a passing score. That's not a real pass.
- **Module boundary hard gate**: this is the rule the client's standards team
  explicitly said they will not support the Claude Code rollout without (Section 2).
  A soft check ("mostly respects boundaries") is exactly the kind of leniency that
  let the original failure mode through in the first place — the entire point of
  making it a hard gate is that "one small boundary violation, otherwise great
  code" is still a FAIL, not a 90/100.
- **Error contract hard gate**: a single raw `throw new Exception` anywhere in a
  Service or Controller means the `ExceptionHandlingMiddleware`'s guarantee (every
  error response has a consistent `{ code, message }` shape) is already false for
  at least one code path. A guarantee that's true 95% of the time isn't a
  guarantee an API consumer can build against.

### Verdict rule walkthrough: variable Generator output → definitive verdict

Sprint 1's actual run: the Generator's self-check in `generator-summary.md` flagged
AC5 (the `UpdatedAt` timestamp assertion) as CONDITIONAL rather than a clean PASS —
the Generator's own confidence was genuinely mixed on this one criterion. The
Evaluator's mechanical checks (build, test run, import scan, `throw` scan) all
passed cleanly and are recorded as hard-gate MET. Dimension 6 (test quality) is the
only dimension where the Evaluator's own reading was needed rather than a tool
output, and it scored 80/100 (one distinct gap — the missing `UpdatedAt` assertion
— per the "−20 per distinct issue" scoring rule in `evaluation-criteria/SKILL.md`),
giving a weighted total of 98.0. No hard gate failed, and the total is ≥85, so the
verdict rule in `evaluator.agent.md` produces **PASS**, deterministically — the
exact same inputs (these six dimension scores, these hard-gate results) will always
produce PASS, regardless of which run of Claude Code generated the Evaluator's
prose explaining it.

### Escalation path

Trigger: three consecutive FAIL verdicts on the same sprint contract. Output:
`.harness/output/escalation.md`, containing the sprint ID, the contract, the
iteration count, and the specific hard-gate failure(s) from the third
`evaluator-feedback.md` (file + line, not a summary). Recipient: the developer who
sent the original `@planner` prompt — CLAUDE.md's orchestration loop stops and
returns control at that point rather than trying a fourth time. Sprint 1 in this
repository did not escalate (PASS on iteration 1 of 3), so `escalation.md` does not
exist in this run; its trigger and required contents are specified in
`CLAUDE.md` Section 4 and exercised only when a sprint genuinely fails three times.

---

## Section D — Architectural Decisions

### Decision 1: Restrict bulk status transitions to `Done`/`Blocked` only

**Alternatives considered**: (a) allow any `ActivityStatus` value in the bulk
endpoint, matching the single-activity `PATCH /{id}` endpoint's flexibility; (b)
restrict to `Done`/`Blocked` only, as implemented.

**Rationale**: the feature's actual use case (Section 3.4 of the case study) is an
outgoing shift worker closing out their queue. Allowing the same bulk mechanism to
move activities *back* to `Todo` or `InProgress` would let a single request
silently mass-reopen or mass-reassign work with no per-activity confirmation — a
much higher-blast-radius action than the feature was asked for. Restricting the
allow-list keeps the endpoint's power proportional to its stated purpose.

**Assumption this depends on**: that re-opening or re-queuing work is rare enough,
and consequential enough, that it's acceptable to force it through the
single-activity endpoint even if that's occasionally less convenient for a
legitimate bulk re-queue scenario. If that assumption turns out to be wrong (bulk
re-queuing becomes a common real workflow), the fix is a new, separately-named
endpoint with its own acceptance criteria — not loosening this one's allow-list.

### Decision 2: Name the `Task` entity `Activity` in code

**Alternatives considered**: (a) use `Task` as specified verbatim in Section 3.3 of
the case study; (b) rename to `Activity` in the implementation, keeping `Task` only
as the spec's domain vocabulary.

**Rationale**: `System.Threading.Tasks.Task` is used in the return type of nearly
every method in an async C# codebase (`Task<Activity>`, `Task UpdateAsync(...)`).
A domain type also named `Task` in the same namespace creates constant ambiguity
that either needs a `using StoreOpsTask = ...` alias everywhere or fully-qualified
names everywhere — both worse for readability than a one-time rename.

**Assumption this depends on**: that a reviewer comparing this implementation
against the original case study document will accept a documented naming deviation
rather than expecting a literal type named `Task`. This assumption is made explicit
here and in `app-context/SKILL.md` specifically so it's never a silent surprise.

### Decision 3: Partial-failure semantics instead of all-or-nothing for the bulk endpoint

**Alternatives considered**: (a) wrap the whole bulk operation in a
transaction-like all-or-nothing check — if any id is invalid, reject the entire
request; (b) partial failure — apply what can be applied, report the rest, as
implemented (AC2).

**Rationale**: an outgoing shift worker's batch is very likely to include at least
one stale id (an activity someone else already closed out, or that was deleted,
during the same shift). All-or-nothing would mean one stale id blocks every other
legitimately-closeable activity in the batch — exactly the friction the bulk
endpoint exists to remove. Partial failure with an explicit `Failed` list keeps the
endpoint useful under realistic conditions while still surfacing the failure rather
than silently swallowing it.

**Assumption this depends on**: that the caller (a shift-handover UI, presumably)
will surface the `Failed` list to the user rather than ignoring it — this
implementation's contract is only as good as the client that consumes it. If a
future requirement needs strict all-or-nothing semantics for a *different*
bulk operation, that would be a new sprint contract with a different AC2, not a
change to this one.
