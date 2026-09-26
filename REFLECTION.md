# REFLECTION.md — Sprint 1 Demonstration Run

## What the harness did well

The skill-file split by agent (Generator gets "how to comply," Evaluator gets "how
to check") kept each agent's context genuinely scoped to its job. When I traced
back through `generator-summary.md` and `evaluator-feedback.md` side by side, the
Evaluator's findings line up cleanly with the Generator's own self-reported "Known
gaps" — the Generator already knew AC5's timestamp assertion was thin before the
Evaluator flagged it, which is a good sign that `how-to-test/SKILL.md`'s
"state-not-just-return-value" instruction was specific enough to be self-checkable,
not just externally checkable.

The hard-gate design also did what it was meant to do in this run: three of the six
dimensions resolved to a clean binary MET/NOT-MET from tool output alone (build,
test, the import/throw scans), which meant the Evaluator's own judgment was only
load-bearing for one dimension (test quality) — exactly the ratio the "automated
checks are deterministic, LLM judgment is the smaller remainder" design goal in
Section 5.4 of the case study was asking for.

## Where it fell short

The harness caught the `UpdatedAt`-assertion gap (Dimension 6, 80/100) but let it
through as a CONDITIONAL-weight shortfall rather than a hard gate — which was the
right call per the rubric (it's a real gap, not a rule violation), but it means a
genuinely incomplete test suite can still produce an overall PASS. That's a
deliberate design trade-off (see DESIGN_BRIEF.md Section C on why test quality
carries the smallest weight of the six), but running it for real made the trade-off
concrete in a way that just designing the rubric on paper didn't: I now have an
actual sprint where "PASS" doesn't mean "nothing left to improve," and the
harness's audit trail is honest about that rather than hiding it — the gap is named
explicitly in `evaluator-feedback.md`, not glossed over.

The second shortfall: `generator-summary.md` flags that the Alerts side effect
(the actual `Notification` a shift worker would see) is exercised by the event
publish but never asserted by name in the test suite. The Evaluator's event-bus
hard gate checks *that* the side effect is raised via the bus correctly — it does
not currently check *that the subscriber actually does something useful with it*.
That's a real blind spot: a subscriber that silently no-ops would still pass
Dimension 3.

## One concrete improvement

Add a seventh check to `how-to-review/SKILL.md` (and a corresponding line in
`evaluation-criteria/SKILL.md`'s Dimension 3): when a sprint's `generator-summary.md`
declares a cross-module event publish, the Evaluator should also require at least
one test that asserts the *subscriber's* resulting state changed — not just that
`IEventBus.Publish` was called. This closes the exact blind spot found in this
sprint (an event bus call that's correctly wired but whose downstream effect is
unverified) and would have caught the missing Alerts-side test before it became a
"Known gap" instead of a hard-gate finding.
