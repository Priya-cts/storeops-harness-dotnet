# SKILL: architecture-principles

## Purpose

The non-negotiable StoreOps architecture rules. These map directly to the four
failure modes the client's engineering standards team observed in a prior
AI-assisted experiment (see Section 2 of the programme case study). Every hard gate
in `evaluation-criteria/SKILL.md` traces back to one of the four rules below. Read by
Planner, Generator, and Evaluator.

## Rule 1 — Layer separation

`Routes (Controller) → Service → Repository`. No skipping, no reversing.

- Controllers: HTTP concerns and DTO (de)serialization only. No business logic, no
  direct repository access.
- Services: all business logic and validation. This is the only layer allowed to
  throw `AppError`s or call `IEventBus.Publish`.
- Repositories: data access only. No HTTP types, no calls to other modules' services.

*What breaks without it*: in the client's prior experiment, generated code put
business rules directly in route handlers, which meant the same rule had to be
re-implemented (and inevitably drifted) wherever the endpoint was called from a
second place.

## Rule 2 — Module boundary

No module's Repository or internal `Models` types may be imported by another
module. A module that needs data from another module calls that module's public
`I*Service` interface — never its repository, never its concrete model classes
directly (only the DTOs / shared event records it's meant to see).

*Example in this codebase*: `Reports.ReportService` depends on
`Activities.IActivityService`, not on `Activities.IActivityRepository` or
`Activities.Models.Activity` internals beyond what `IActivityService` already
returns.

*What breaks without it*: direct repository imports were failure mode #1 in the
client's prior experiment — they silently coupled modules' storage schemas
together, so changing one module's repository broke a second module at compile
time (or worse, at runtime with in-memory storage).

## Rule 3 — Event bus only

Any side effect that crosses a module boundary — a notification, an audit record,
a downstream report trigger — is raised via `IEventBus.Publish(eventType, payload)`,
never by one module directly instantiating or injecting another module's service.

- Event payloads are typed records defined in `Shared/Events/DomainEvents.cs`, not
  ad-hoc anonymous objects, and not the publishing module's own internal `Models`
  types — this is what lets the subscribing module depend on a contract instead of
  an implementation.
- Subscriptions are registered once, at startup, in `Program.cs` (see
  `AlertEventSubscriptions.Register(...)`).

*What breaks without it*: failure mode #4 in the client's prior experiment — a
generated feature wrote directly into a sibling module's repository to raise a
notification, which meant Activities and Alerts could no longer be deployed,
tested, or reasoned about independently.

## Rule 4 — Error contract

No `throw new Exception(...)` or `throw new Error(...)` anywhere in a Service or
Controller. Every thrown error is an `AppError` subclass (`NotFoundError`,
`ValidationError`, `ConflictError`, `ForbiddenError`, or a new subclass added to
`Shared/Errors/AppError.cs` if a genuinely new error shape is needed). The
`ExceptionHandlingMiddleware` is the only place that translates an `AppError` into
an HTTP response.

*What breaks without it*: failure mode #2 in the client's prior experiment — raw
`Error` throws bypassed the agreed `{ code, message, statusCode }` response shape,
so API consumers got inconsistent error payloads depending on which developer (or
which AI generation run) wrote a given endpoint.

## Rule 5 — Read-only Reports

`Reports` may call other modules' service layers for read-only aggregation. It must
never write to `Activities`, `Programmes`, or `Staff` — no calls to any of their
`Create*`, `Update*`, `Delete*`, or `BulkUpdate*` service methods.

## Rule 6 — Testing thresholds

| Scope | Minimum line coverage |
|---|---|
| Service layer | 80% |
| Route/Controller layer | 70% |
| Shared utilities | 60% |
| Overall project | 70% |

Tests must verify business-rule compliance, not just HTTP status codes — failure
mode #3 in the client's prior experiment was tests that asserted `200 OK` without
checking the actual state change or event side effect. A test for
`BulkUpdateStatusAsync`, for instance, must assert both the returned
`Updated`/`Failed` split *and* that a re-fetched activity actually has the new
status — see `ActivityServiceTests.BulkUpdateStatusAsync_WithMixedIds_...`.
