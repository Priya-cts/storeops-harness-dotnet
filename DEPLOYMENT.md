# DEPLOYMENT.md

## Build and test verification

```
PS> dotnet --version
10.0.401

PS> dotnet build
Restore complete (1.5s)
  StoreOps.Api net8.0 succeeded (5.2s) → src\StoreOps.Api\bin\Debug\net8.0\StoreOps.Api.dll
  StoreOps.Tests net8.0 succeeded (2.1s) → tests\StoreOps.Tests\bin\Debug\net8.0\StoreOps.Tests.dll

Build succeeded in 9.6s

PS> dotnet test
...
[xUnit.net 00:00:00.56]   Starting:    StoreOps.Tests
[xUnit.net 00:00:02.16]   Finished:    StoreOps.Tests
  StoreOps.Tests test net8.0 succeeded (5.2s)

Test summary: total: 9, failed: 0, succeeded: 9, skipped: 0, duration: 5.2s
Build succeeded in 7.6s
```

**Result: build succeeded with 0 errors; all 9 tests passed, 0 failed.** The logged
`AppError handled: VALIDATION_ERROR` line visible mid-run is expected — it is the
`ActivityServiceTests.BulkUpdateStatusAsync_WithInvalidStatus_ThrowsValidationError`
test deliberately exercising the error-contract path (Rule 4 in
`architecture-principles/SKILL.md`); the `ExceptionHandlingMiddleware` correctly
caught and logged the `ValidationError` it was designed to catch. It is not a test
failure.

## Deployment target: Local run (`dotnet run`)

Docker Desktop was installed but not used for the evidence below — this
deployment target instead satisfies the Section 3.4 requirement directly
("call the new endpoint via curl or Postman and show a successful response")
via `dotnet run`, without the extra Docker packaging layer.

### Step 1 — Run the application

```powershell
dotnet run --project src\StoreOps.Api
```

Application started and listened on `http://localhost:5000`.

### Step 2 — Create an activity

```powershell
$body = @{
    title = "Restock shelf 4"
    programmeId = "11111111-1111-1111-1111-111111111111"
    category = "Restocking"
} | ConvertTo-Json

$created = Invoke-RestMethod -Uri "http://localhost:5000/api/activities" -Method Post -ContentType "application/json" -Body $body
$created
```

**Response:**
```
id          : bfd85d1c-83d5-41de-bddb-6ffd755c463b
title       : Restock shelf 4
description :
programmeId : 11111111-1111-1111-1111-111111111111
assigneeId  :
status      : 0
priority    : 1
category    : 0
dueDate     :
createdAt   : 2026-09-26T14:37:29.9102903Z
updatedAt   : 2026-09-26T14:37:29.9102917Z
```

### Step 3 — Bulk status update (the Sprint 1 demonstration feature)

```powershell
$bulkBody = @{
    activityIds = @($created.id)
    status = "Done"
    actorId = "22222222-2222-2222-2222-222222222222"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/activities/bulk-status" -Method Patch -ContentType "application/json" -Body $bulkBody
```

**Response:**
```
updated                                failed
-------                                ------
{bfd85d1c-83d5-41de-bddb-6ffd755c463b} {}
```

This confirms **AC1** from `sprint-1-contract.md` end-to-end through the running
application: the activity created in Step 2 was successfully closed out via the
harness-governed bulk endpoint, returned under `updated`, with `failed` empty as
expected for an all-valid-ids request.

## Deployment target: Local Docker (attempted, not completed)

Docker Desktop 4.92.0 was installed successfully on the development machine. The
`Dockerfile` and `docker-compose.yml` in this repository are written and ready to
use (`docker compose up --build`, then the same requests above against port
`8080` instead of `5000`), but a full container run was not completed in time for
this submission. The local `dotnet run` evidence above was used as the deployment
target instead, which satisfies the case study's minimum requirement in Section
3.4 without requiring Docker specifically.

## Deployment target: Cloud

Not attempted — out of scope for this submission given time constraints. Local
run was sufficient to demonstrate the governed feature end-to-end (see above).

## Environment configuration and secrets

None required. All storage in this capstone is in-memory; no external services,
databases, or API keys are called by the application.
