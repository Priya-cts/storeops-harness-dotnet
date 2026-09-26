namespace StoreOps.Api.Modules.Reports;

using StoreOps.Api.Modules.Activities;
using StoreOps.Api.Modules.Activities.Models;
using StoreOps.Api.Modules.Reports.Models;

// Read-only by architecture rule (Section 3.5): Reports may call another module's
// SERVICE layer for lookups, but must never write to activities/programmes/staff.
// Not exposed over HTTP in this demonstration run (the regional-rollup endpoint
// from Section 3.4 was out of scope — see REFLECTION.md); kept as a stub that
// proves the read-only cross-module call pattern compiles and is testable.
public sealed class ReportService : IReportService
{
    private readonly IActivityService _activityService;

    public ReportService(IActivityService activityService) => _activityService = activityService;

    public async Task<StoreSummaryReport> GetStoreSummaryAsync(Guid storeId, Guid programmeId)
    {
        var activities = await _activityService.ListAsync(programmeId, status: null);

        return new StoreSummaryReport
        {
            StoreId = storeId,
            TotalActivities = activities.Count,
            CompletedActivities = activities.Count(a => a.Status == ActivityStatus.Done),
            OverdueActivities = activities.Count(a =>
                a.DueDate.HasValue && a.DueDate.Value < DateTime.UtcNow && a.Status != ActivityStatus.Done)
        };
    }
}
