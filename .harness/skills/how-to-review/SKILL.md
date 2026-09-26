# SKILL: how-to-review

## Purpose

How the Evaluator turns a code diff and `generator-summary.md` into the structured
sections required by `evaluator.agent.md`. Read by the Evaluator only.

## Review order

1. **Run the automated checks first.** `dotnet build && dotnet test` (from the repo
   root). These are the cheapest, most deterministic signal and gate two of the six
   dimensions in `evaluation-criteria/SKILL.md` on their own. Do not hand-review
   code that doesn't even compile.
2. **Check module boundaries** by scanning `using` statements in every changed file
   under `src/StoreOps.Api/Modules/<X>/`: any `using StoreOps.Api.Modules.<Y>...`
   where `Y != X` is only acceptable if the imported type is `I<Y>Service` (or a
   DTO explicitly meant to cross the boundary, or a `Shared/Events` record) — a
   direct import of `<Y>.Models` or `I<Y>Repository` from module `X` is an
   automatic hard-gate FAIL.
3. **Check the error contract** by scanning changed Service/Controller files for
   `throw new` — every match must construct an `AppError` subclass. A bare
   `throw new Exception(` or `throw new ArgumentException(` etc. is an automatic
   hard-gate FAIL.
4. **Check event-bus usage** for any feature that the sprint contract or
   `generator-summary.md` describes as crossing a module boundary at runtime (not
   just at compile time) — confirm the side effect is raised via
   `IEventBus.Publish(...)` with a typed record from `Shared/Events`, not a direct
   call into another module's service for a *write* operation.
5. **Check layer separation** — Controllers should contain no `if`/business-rule
   branching beyond simple null/empty guards before delegating to a service;
   Repositories should contain no reference to `Shared.Errors` or `Shared.Events`
   (a repository that throws a `ValidationError` or publishes an event is a layer
   violation, because that's the Service's job).
6. **Check DI lifetimes** in `Program.cs` for anything touched by this sprint —
   repositories must stay `Singleton`, services `Scoped` (see
   `coding-conventions/SKILL.md`).
7. **Check test quality**, not just presence — do the tests assert resulting state
   (per `how-to-test/SKILL.md`), or only the immediate return value / status code?
   A test suite that only checks `response.StatusCode == 200` for a state-mutating
   endpoint fails the "business rule compliance" check even if `dotnet test` is
   green.

## Writing file+line feedback

Every FAIL or CONDITIONAL item in `evaluator-feedback.md` names the exact file and,
where possible, the line number — e.g. `src/StoreOps.Api/Modules/Reports/
ReportService.cs:14 — imports StoreOps.Api.Modules.Activities.Models.Activity
directly; use IActivityService's returned DTO instead.` A Generator retry cannot act
on "module boundaries need work" as feedback; it can act on a specific file and line.

## Ambiguity handling

If the Evaluator cannot determine with confidence whether a check passes (e.g. an
import's purpose isn't obvious from the code alone), it does not guess PASS. It
records the check as FAIL with a note explaining what made it ambiguous, and — if
the ambiguity is about intent rather than a clear rule violation — recommends the
Planner clarify the spec rather than the Generator blindly retrying. This is the
harness's fallback for LLM assessor leniency: uncertainty defaults to the stricter
outcome, never the more permissive one.
