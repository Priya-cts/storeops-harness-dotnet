# SKILL: coding-conventions

## Purpose

Stack-specific rules the Generator applies when writing C# code for StoreOps. Read
by the Generator only.

## Naming

- Interfaces: `I` prefix (`IActivityService`, `IActivityRepository`).
- Async methods: `Async` suffix, always return `Task` or `Task<T>`, always awaited
  (no `.Result`, no `.Wait()`, no `async void` outside test setup).
- Enums: PascalCase members, no `None`/`Unknown` filler values unless the domain
  genuinely has one — StoreOps enums map 1:1 to the retail vocabulary in
  `app-context/SKILL.md` (e.g. `ActivityStatus.Blocked`, not `ActivityStatus.Status3`).
- DTOs live in a module's `Dtos/` folder and are named `<Verb><Noun>Request` /
  `<Noun>Result` (`CreateActivityRequest`, `BulkStatusUpdateResult`) — never reuse a
  domain `Models` type as a request/response DTO directly.

## Nullability

`<Nullable>enable</Nullable>` is set project-wide. A property is `required` if the
domain genuinely cannot exist without it (`Activity.Title`), nullable (`string?`) if
it's genuinely optional (`Activity.Description`), and never given a silent empty-
string or sentinel-value default to dodge the nullable warning.

## Enum parsing from request DTOs

Incoming `string` fields that map to enums (`Status`, `Priority`, `Category`, `Role`)
are parsed with `Enum.TryParse<T>(value, ignoreCase: true, out var parsed)`, never
`Enum.Parse<T>(value)` unguarded — an unrecognized value must become a
`ValidationError` with the offending value in the message, not an unhandled
`ArgumentException` that the `ExceptionHandlingMiddleware` would have to catch as a
generic 500.

## Concurrency

In-memory repositories use a private `readonly object _lock` and `lock (_lock) { }`
around all dictionary/list mutation and iteration. This project has no database, so
this lock is the only thing preventing a torn read during a bulk operation — do not
remove it "because it's just a demo."

## Controllers

- One controller per module, routed at `api/<module-plural-lowercase>`.
- Route-parameter-based actions use a type constraint (`{id:guid}`) so that a
  literal path segment like `bulk-status` on the same controller can never be
  ambiguously matched against an `{id:guid}` route — see
  `ActivitiesController.BulkUpdateStatus` for the pattern.
- Controllers never catch `AppError` themselves; they let it propagate to
  `ExceptionHandlingMiddleware`.

## Dependency injection lifetimes

- Repositories: `Singleton` (they hold the actual in-memory state for the process
  lifetime).
- Services: `Scoped` (one instance per HTTP request; stateless beyond what's
  injected).
- `IEventBus`: `Singleton` (subscriptions registered once at startup must persist).

Getting a repository's lifetime wrong (e.g. registering it `Scoped`) is a common,
easy-to-miss bug in this codebase specifically because it compiles and even passes a
naive smoke test — a `Scoped` repository would silently "forget" data between
requests. The Evaluator's hard gates check for this explicitly (see
`evaluation-criteria/SKILL.md`).
