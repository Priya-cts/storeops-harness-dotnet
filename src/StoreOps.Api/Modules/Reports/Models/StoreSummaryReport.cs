namespace StoreOps.Api.Modules.Reports.Models;

public enum ReportType { StoreSummary, RegionalRollup, DepartmentPerformance }
public enum ReportStatus { Pending, Ready, Failed }

public sealed class StoreSummaryReport
{
    public Guid StoreId { get; init; }
    public int TotalActivities { get; init; }
    public int CompletedActivities { get; init; }
    public int OverdueActivities { get; init; }
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
}
