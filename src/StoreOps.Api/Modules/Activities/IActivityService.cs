namespace StoreOps.Api.Modules.Activities;

using StoreOps.Api.Modules.Activities.Dtos;
using StoreOps.Api.Modules.Activities.Models;

public interface IActivityService
{
    Task<Activity> GetByIdAsync(Guid id);
    Task<List<Activity>> ListAsync(Guid? programmeId, string? status);
    Task<Activity> CreateAsync(CreateActivityRequest request);
    Task<Activity> UpdateAsync(Guid id, UpdateActivityRequest request);
    Task DeleteAsync(Guid id);
    Task<BulkStatusUpdateResult> BulkUpdateStatusAsync(BulkStatusUpdateRequest request);
}
