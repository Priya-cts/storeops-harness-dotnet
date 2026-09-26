# SKILL: how-to-test

## Purpose

How the Generator writes tests that satisfy Rule 6 (testing thresholds) and the
"business rule compliance, not just status codes" requirement in
`architecture-principles/SKILL.md`. Read by the Generator only.

## Test project layout

`tests/StoreOps.Tests/<Module>/` mirrors `src/StoreOps.Api/Modules/<Module>/`.
Service-layer tests and endpoint-level tests are separate files in the same folder
(`ActivityServiceTests.cs`, `BulkStatusEndpointTests.cs`).

## Service-layer tests (xUnit + FluentAssertions, no HTTP)

- Construct the service directly against `InMemoryActivityRepository` (or the
  equivalent for the module under test) and `InMemoryEventBus`, using
  `NullLogger<T>.Instance` for the logger dependency — no mocking framework needed
  for this codebase's dependency graph.
- Every acceptance criterion in the sprint contract gets at least one test named
  `<Method>_<Scenario>_<ExpectedOutcome>` (e.g.
  `BulkUpdateStatusAsync_WithMixedIds_ReturnsPartialResult`).
- A test that exercises a state change (a status update, a bulk operation) must
  assert the *resulting state*, not just the return value — re-fetch the entity
  through the service and assert its new state, exactly as
  `BulkUpdateStatusAsync_WithMixedIds_ReturnsPartialResult` does.
- Error-path tests assert the specific `AppError` subclass thrown
  (`ThrowAsync<ValidationError>()`), not a generic `Exception`.

## Endpoint-level tests (`WebApplicationFactory<Program>`)

- Used to prove the full Routes → Service → Repository chain works end-to-end
  through real HTTP, including the `ExceptionHandlingMiddleware` translating an
  `AppError` into the right status code.
- `HttpClient.PostAsJsonAsync` / `GetFromJsonAsync` cover POST/GET. `PATCH` has no
  built-in `*AsJsonAsync` helper in `System.Net.Http.Json`, so build an
  `HttpRequestMessage` with `HttpMethod.Patch` and `JsonContent.Create(...)`
  explicitly — see `BulkStatusEndpointTests`.
- At least one endpoint test per new route asserts the *error* path returns the
  expected status code (e.g. an invalid `Status` string on `bulk-status` should
  return `400`, proving the `ValidationError → 400` mapping actually works over
  real HTTP, not just in a unit test of the middleware in isolation).

## Coverage expectation for a new sprint

A sprint contract with N acceptance criteria should produce roughly N+2 to N+4 test
cases once you include: one happy-path test per criterion, at least one
partial-failure or error-path test, and one endpoint-level test proving the wiring.
Fewer than that is a signal (to the Generator itself, before the Evaluator even
looks) that an edge case was probably skipped.
