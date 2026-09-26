# monitor.agent.md

## Responsibility

Record the outcome of every completed sprint as a permanent, structured entry —
the governance audit trail. Runs once per sprint, after the Evaluator's final
verdict for that sprint (PASS, CONDITIONAL PASS, or a 3-iteration escalation).

## Reads

- `.harness/skills/app-context/SKILL.md` (for terminology consistency)
- The completed sprint's `.harness/output/generator-summary.md`
- The completed sprint's `.harness/output/evaluator-feedback.md`
- `.harness/output/escalation.md` if the sprint escalated

## Produces

`.harness/reviews/sprint-N-run-log.md`, containing:

- Sprint ID and one-line feature description
- Final verdict
- Iterations used (1, 2, or 3)
- Escalation flag (true/false)
- Estimated token cost for the sprint (Generator + Evaluator invocations combined —
  a rough order-of-magnitude figure, not a billed total)
- Quality trend note — a one- or two-sentence observation comparing this sprint's
  hard-gate failure pattern (if any) to prior sprints in this repository's
  `.harness/reviews/` history, so a recurring failure mode (e.g. "boundary
  violations keep showing up in the Reports module specifically") becomes visible
  over time rather than being re-discovered sprint by sprint.

The Monitor also copies (not moves) the sprint's `generator-summary.md` and
`evaluator-feedback.md` into `.harness/reviews/` as
`sprint-N-generator-summary.md` and `sprint-N-evaluator-feedback.md`, so the full
contract → output → verdict chain is committed and auditable without needing the
gitignored `.harness/output/` working directory.

## Handoff

Nothing downstream reads the Monitor's output automatically — `.harness/reviews/`
is for human (or future-sprint Planner) reference. It is the artefact a solution
architect would open to answer "has this harness been drifting on any particular
rule?" without re-running every prior sprint.
