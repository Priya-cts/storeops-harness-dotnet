# SKILL: evaluation-criteria

## Purpose

The Evaluator's scoring rubric: dimensions, weights, and hard gates. This is what
converts a Generator's inherently variable output into the deterministic PASS /
CONDITIONAL PASS / FAIL verdict rule defined in `evaluator.agent.md`. Read by the
Evaluator only.

## Dimensions (weights sum to 100%)

| # | Dimension | Weight | Hard gate |
|---|---|---|---|
| 1 | Build & test correctness | 30% | `dotnet build` exits 0 AND `dotnet test` reports 0 failures |
| 2 | Module boundary compliance | 20% | Zero disallowed cross-module imports (Rule 2) |
| 3 | Event bus compliance | 15% | Zero direct write-calls into another module's service for a cross-boundary side effect (Rule 3) |
| 4 | Error contract compliance | 15% | Zero raw `throw new Exception`/`Error` in Service or Controller code (Rule 4) |
| 5 | Layer separation | 10% | No business logic in Controllers; no `Shared.Errors`/`Shared.Events` usage in Repositories |
| 6 | Test quality & coverage | 10% | Coverage thresholds from `architecture-principles/SKILL.md` Rule 6 met, AND at least one test per changed endpoint asserts resulting state (not status code alone) |

Each hard gate is **automated or mechanically checkable** — this is deliberate.
Dimension 1's gate is a literal exit code. Dimensions 2–5's gates are checked by
scanning `using` statements and `throw` sites (a static, repeatable procedure — see
`how-to-review/SKILL.md`), not by an open-ended "does this look architecturally
sound" judgment call. Dimension 6's coverage half is a number from the test runner's
coverage output; only the "asserts resulting state" half requires the Evaluator's
own reading, which is why it carries the smallest weight of the six.

## Why these six dimensions, for StoreOps specifically

Dimensions 2, 3, and 4 map 1:1 onto the three code-shaped failure modes from the
client's prior experiment (Section 2 of the programme case study): direct
cross-module repository imports, raw error throws, and missing event-bus
integration. Dimension 6 maps onto the fourth failure mode: tests that asserted
HTTP status but not business-rule compliance. Dimensions 1 and 5 are included
because a harness that only checked the four client-reported failure modes would
still let through code that doesn't compile, or that technically avoids all four
named failure modes while still putting business logic in a Controller — a fifth
failure mode nobody had reported yet, but that the layer-separation rule exists
specifically to prevent.

## Scoring a dimension without a hard gate firing

For dimensions with a partially-automated gate (5 and 6), a dimension's raw score
is 100 if the gate condition is fully met, and drops in increments of 20 per
distinct issue found (e.g. two Controllers with minor business logic → 60/100 for
Dimension 5), floored at 0. This keeps the non-hard-gate scoring itself
deterministic given the same set of findings, rather than an unbounded subjective
number.

## Verdict rule

Restated from `evaluator.agent.md` for completeness:

- **Any** hard gate fails → verdict is **FAIL**, full stop, regardless of the
  weighted total. A hard gate failure represents a rule the client's standards team
  will not support the Claude Code rollout without (Section 2 of the case study) —
  it cannot be averaged away by good scores elsewhere.
- No hard gate fails, weighted total ≥ 85 → **PASS**.
- No hard gate fails, weighted total 70–84 → **CONDITIONAL PASS** — sprint advances,
  but the specific shortfall is recorded in `run-log.md` for the Monitor to surface
  as a trend if it recurs.
- No hard gate fails, weighted total < 70 → **FAIL**.

## Worked example (Sprint 1, this repository)

See `.harness/reviews/sprint-1-evaluator-feedback.md` for the actual verdict this
rubric produced against the bulk-status demonstration feature — dimensions 1–5 all
scored 100 (all hard gates passed cleanly), dimension 6 scored 80 because one
partial-failure edge case (an empty `ActivityIds` list against a *valid* status
value) had contract coverage but no dedicated endpoint-level test, for a weighted
total of 98 and a verdict of **PASS**.
