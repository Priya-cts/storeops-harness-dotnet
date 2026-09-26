namespace StoreOps.Api.Modules.Activities;

using StoreOps.Api.Modules.Activities.Models;

public interface IActivityRepository
{
    Task<Activity?> GetByIdAsync(Guid id);
    Task<List<Activity>> ListAsync(Guid? programmeId, ActivityStatus? status);
    Task<Activity> AddAsync(Activity activity);
    Task UpdateAsync(Activity activity);
    Task<bool> DeleteAsync(Guid id);
}
