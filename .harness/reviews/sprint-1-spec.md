# spec.md — Sprint Plan: Shift Handover Bulk Update

## Feature (plain language)

Outgoing shift staff need to close out multiple operational activities at once
instead of updating each one individually through `PATCH /api/activities/{id}`.
This adds a bulk endpoint that accepts a list of activity ids and a single target
status (`Done` or `Blocked`), applies it to every activity it can find, and reports
which ones it couldn't update rather than failing the whole request.

## Endpoint

`PATCH /api/activities/bulk-status`

## Module(s) involved

- **Activities** (primary — new service method, new controller action, new DTOs)
- **Alerts** (secondary — receives an audit/notification side effect per updated
  activity)

## Architecture-rule implications

This feature crosses the Activities → Alerts module boundary at runtime (each
successful status change should leave an auditable trace visible to the acting
staff member). Per Rule 3 (event bus only) in `architecture-principles/SKILL.md`,
this **must** be implemented as `IEventBus.Publish("ACTIVITY_BULK_STATUS_CHANGED",
...)` from the Activities service, with Alerts subscribing at startup — not as a
direct call from `ActivityService` into `IAlertService`/`IAlertRepository`.

The target status is restricted to `Done` or `Blocked` only (not `Todo` or
`InProgress`) — a bulk shift-handover action closing out activities should not be
usable to silently re-open or re-queue work; that stays a single-activity action
via the existing `PATCH /api/activities/{id}`.

## Sprint plan

One sprint. The feature is a single cohesive Routes → Service → Repository change
plus one cross-module event contract — it does not need to be split further (see
sprint-sizing rule in `planner.agent.md`).

STATUS: AWAITING APPROVAL
