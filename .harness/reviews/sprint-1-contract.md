# sprint-1-contract.md — Shift Handover Bulk Update

## Acceptance Criteria

**AC1**
GIVEN a request body with one or more valid, existing `activityIds` and a `status`
of `"Done"` or `"Blocked"`
WHEN `PATCH /api/activities/bulk-status` is called
THEN every named activity's status is updated to the requested value, the response
is `200 OK` with each id listed under `Updated`, and one
`ACTIVITY_BULK_STATUS_CHANGED` event is published per updated activity.

**AC2**
GIVEN a request body where some `activityIds` exist and some do not
WHEN `PATCH /api/activities/bulk-status` is called
THEN the existing activities are updated and listed under `Updated`, the
non-existent ids are listed under `Failed` with reason `"Activity not found."`, and
the response is still `200 OK` (partial failure does not abort the whole request).

**AC3**
GIVEN a request body with `status` set to any value other than `"Done"` or
`"Blocked"` (e.g. `"Todo"`, `"InProgress"`, or a nonsense string)
WHEN `PATCH /api/activities/bulk-status` is called
THEN the request is rejected with `400 Bad Request` and a `ValidationError`-shaped
body, and no activity is modified.

**AC4**
GIVEN a request body with an empty `activityIds` list
WHEN `PATCH /api/activities/bulk-status` is called
THEN the request is rejected with `400 Bad Request` before any lookup is attempted.

**AC5**
GIVEN a successful bulk update (AC1 or the successful portion of AC2)
WHEN the update completes
THEN each updated activity's `UpdatedAt` timestamp reflects the change, confirmed by
re-fetching the activity via `GET /api/activities/{id}` — not just by the bulk
response body.

## Non-goals for this sprint

- No SLA-breach escalation logic (that is the second suggested feature in Section
  3.4 of the case study, out of scope here).
- No pagination or batch-size limit on `activityIds` (acceptable for this capstone's
  scope; flagged in REFLECTION.md as a real-world follow-up).
