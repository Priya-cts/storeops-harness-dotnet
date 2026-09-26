# SKILL: app-context

## Purpose

Give every agent the same shared picture of what StoreOps is, so a Planner sprint
contract, a Generator implementation, and an Evaluator check are all reasoning about
the same domain vocabulary. Read by all four agents.

## What StoreOps is

StoreOps is a retail store operations REST API (.NET 8 / ASP.NET Core Web API, C# 12).
It lets store teams create operational programmes, assign and track activities across
departments, coordinate staff, and surface performance reports by store and region.

## The five modules

| Module | Retail responsibility | Key types |
|---|---|---|
| `Activities` | Restocking runs, planogram resets, compliance checks, general store tasks | `Activity`, `ActivityStatus` (Todo\|InProgress\|Done\|Blocked), `ActivityPriority` (Low\|Medium\|High\|Critical), `ActivityCategory` (Restocking\|Planogram\|Audit\|Compliance\|General) |
| `Programmes` | Store programmes and their staff membership — seasonal rollouts, compliance drives, store refits | `Project`, `ProjectMember`, `ProjectRole` (StoreManager\|DepartmentLead\|Associate) |
| `Staff` | Store staff registration and profile lookups (auth-only — no CRUD HTTP surface) | `User`, `StaffRole` (RegionalManager\|StoreManager\|DepartmentLead\|Associate) |
| `Alerts` | In-app alerts triggered by operational events | `Notification`, `NotificationChannel` (InApp\|Email), `NotificationStatus`, `AlertType` (Inventory\|SlaBreach\|ShiftHandover\|Escalation) |
| `Reports` | Read-only store/regional performance aggregation | `StoreSummaryReport`, `ReportType`, `ReportStatus` |

> Naming note: the spec's `Task` entity is implemented as `Activity` in this codebase
> to avoid colliding with `System.Threading.Tasks.Task`, used throughout every async
> method here. See DESIGN_BRIEF.md Section D for the full rationale.

## Base API surface (already implemented, do not re-generate)

```
GET    /api/activities              (optional programmeId, status filters)
POST   /api/activities
GET    /api/activities/{id}
PATCH  /api/activities/{id}
DELETE /api/activities/{id}
GET    /api/programmes              (requires storeId)
POST   /api/programmes
POST   /api/programmes/{id}/members
GET    /api/alerts                  (requires userId)
```

## Demonstration feature already governed by this harness

```
PATCH /api/activities/bulk-status
```
Bulk-marks multiple activities DONE or BLOCKED in one request (shift handover),
partial-failure semantics, one `ACTIVITY_BULK_STATUS_CHANGED` event per successful
update. Implemented in Sprint 1 — see `.harness/reviews/sprint-1-*`.

## Storage

In-memory (`Dictionary`-backed repositories, one per module, registered as
singletons). No database in this capstone scope — see DEPLOYMENT.md.
