namespace StoreOps.Api.Modules.Activities;

using StoreOps.Api.Modules.Activities.Models;

public sealed class InMemoryActivityRepository : IActivityRepository
{
    private readonly Dictionary<Guid, Activity> _store = new();
    private readonly object _lock = new();

    public Task<Activity?> GetByIdAsync(Guid id)
    {
        lock (_lock)
        {
            _store.TryGetValue(id, out var activity);
            return Task.FromResult(activity);
        }
    }

    public Task<List<Activity>> ListAsync(Guid? programmeId, ActivityStatus? status)
    {
        lock (_lock)
        {
            var query = _store.Values.AsEnumerable();
            if (programmeId.HasValue) query = query.Where(a => a.ProgrammeId == programmeId.Value);
            if (status.HasValue) query = query.Where(a => a.Status == status.Value);
            return Task.FromResult(query.ToList());
        }
    }

    public Task<Activity> AddAsync(Activity activity)
    {
        lock (_lock)
        {
            _store[activity.Id] = activity;
            return Task.FromResult(activity);
        }
    }

    public Task UpdateAsync(Activity activity)
    {
        lock (_lock)
        {
            activity.UpdatedAt = DateTime.UtcNow;
            _store[activity.Id] = activity;
            return Task.CompletedTask;
        }
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        lock (_lock)
        {
            return Task.FromResult(_store.Remove(id));
        }
    }
}
