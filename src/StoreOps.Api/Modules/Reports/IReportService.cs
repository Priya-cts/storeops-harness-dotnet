namespace StoreOps.Api.Modules.Reports;

using StoreOps.Api.Modules.Reports.Models;

public interface IReportService
{
    Task<StoreSummaryReport> GetStoreSummaryAsync(Guid storeId, Guid programmeId);
}
