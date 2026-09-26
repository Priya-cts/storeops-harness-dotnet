# generator-summary.md — Sprint 1

## AC self-check table

| AC | Self-assessed | Justification |
|---|---|---|
| AC1 | PASS | `ActivityService.BulkUpdateStatusAsync` updates every found activity and publishes one `ActivityBulkStatusChangedEvent` per update via `IEventBus.Publish`. Covered by `BulkUpdateStatusAsync_WithMixedIds_ReturnsPartialResult` (successful half) and `BulkStatusEndpointTests.PatchBulkStatus_WithCreatedActivity_ReturnsUpdatedList`. |
| AC2 | PASS | Unknown ids are collected into `Failed` with reason `"Activity not found."` instead of throwing; loop continues. Covered by `BulkUpdateStatusAsync_WithMixedIds_ReturnsPartialResult`. |
| AC3 | PASS | `Enum.TryParse` against an allow-list (`Done`/`Blocked` only) throws `ValidationError` for anything else, translated to 400 by `ExceptionHandlingMiddleware`. Covered by `BulkUpdateStatusAsync_WithInvalidStatus_ThrowsValidationError` and `BulkStatusEndpointTests.PatchBulkStatus_WithInvalidStatus_ReturnsBadRequest`. |
| AC4 | PASS | Empty/`null` `ActivityIds` throws `ValidationError` before any repository call. Covered by `BulkUpdateStatusAsync_WithNoIds_ThrowsValidationError`. |
| AC5 | CONDITIONAL | `UpdateAsync` on the repository sets `UpdatedAt = DateTime.UtcNow` on every write, and `BulkUpdateStatusAsync_WithMixedIds_ReturnsPartialResult` re-fetches the activity and asserts its `Status`. It does **not** separately assert the `UpdatedAt` timestamp changed — status is verified, timestamp is not. |

## Files changed, by layer

**Dtos**
- `src/StoreOps.Api/Modules/Activities/Dtos/ActivityDtos.cs` — added
  `BulkStatusUpdateRequest`, `BulkStatusUpdateResult`, `BulkStatusFailure`

**Service**
- `src/StoreOps.Api/Modules/Activities/IActivityService.cs` — added
  `BulkUpdateStatusAsync` to the interface
- `src/StoreOps.Api/Modules/Activities/ActivityService.cs` — implemented
  `BulkUpdateStatusAsync`

**Routes**
- `src/StoreOps.Api/Modules/Activities/ActivitiesController.cs` — added
  `[HttpPatch("bulk-status")]` action

**Shared (event contract)**
- `src/StoreOps.Api/Shared/Events/DomainEvents.cs` — added
  `ActivityBulkStatusChangedEvent` record

**Cross-module wiring**
- `src/StoreOps.Api/Modules/Alerts/AlertEventSubscriptions.cs` — new file,
  subscribes to `ACTIVITY_BULK_STATUS_CHANGED` and writes a `Notification`
- `src/StoreOps.Api/Program.cs` — registers the subscription at startup

**Tests**
- `tests/StoreOps.Tests/Activities/ActivityServiceTests.cs` — 3 new tests for
  `BulkUpdateStatusAsync` (mixed ids, invalid status, empty ids)
- `tests/StoreOps.Tests/Activities/BulkStatusEndpointTests.cs` — new file, 2
  endpoint-level tests (happy path, invalid status)

## Known gaps

- AC5's timestamp assertion is contract-covered but not test-covered at the
  granularity the contract implies (see CONDITIONAL row above). Flagged for the
  Evaluator.
- No dedicated test confirms the Alerts module actually receives a `Notification`
  as a result of the bulk update (the event publish is exercised, but the
  subscriber's side effect is not asserted in this sprint's test suite).
- `activityIds` has no upper bound — a very large batch is processed
  synchronously in a single request/response cycle.
