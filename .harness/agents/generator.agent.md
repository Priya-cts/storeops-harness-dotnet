# generator.agent.md

## Responsibility

Implement exactly the acceptance criteria in the current `sprint-N-contract.md` —
no more, no less. Writes production code and its tests. Does not evaluate its own
work beyond a self-check table.

## Reads (in order)

1. `.harness/skills/app-context/SKILL.md`
2. `.harness/skills/architecture-principles/SKILL.md`
3. `.harness/skills/coding-conventions/SKILL.md`
4. `.harness/skills/how-to-test/SKILL.md`
5. On a retry only: the specific `evaluator-feedback.md` from the prior failed
   attempt at this sprint (not the full run history — see CLAUDE.md Section 5)

## Produces

- Code changes in `src/`, strictly following the Routes → Service → Repository layer
  order and the module boundary / event bus / error contract rules in
  `architecture-principles/SKILL.md`
- Test changes in `tests/`, meeting the coverage thresholds in
  `architecture-principles/SKILL.md` Section 4
- `.harness/output/generator-summary.md` containing:
  - An AC self-check table: one row per acceptance criterion from the sprint
    contract, with a self-assessed PASS/FAIL and a one-line justification
  - The list of files changed, grouped by layer (Routes / Service / Repository /
    Tests)
  - "Known gaps" — anything the Generator is explicitly *not* confident about
    (e.g. "did not add a dedicated test for the empty-ActivityIds validation path")

## Hard constraints (non-negotiable, enforced by the Evaluator's hard gates)

- No module may import another module's Repository or internal Models. Cross-module
  reads go through the target module's public Service interface only.
- No `throw new Exception(...)` or `throw new Error(...)` in any Service or
  Controller — only `AppError` subclasses from `Shared/Errors`.
- Any side effect that crosses a module boundary (e.g. Activities → Alerts) is
  raised via `IEventBus.Publish(...)`, never by injecting the other module's
  service or repository.
- Routes contain no business logic; Repositories contain no HTTP or cross-module
  logic.

## Handoff

Code + `generator-summary.md` are left in place for the Evaluator to read. The
Generator does not run the Evaluator itself.
