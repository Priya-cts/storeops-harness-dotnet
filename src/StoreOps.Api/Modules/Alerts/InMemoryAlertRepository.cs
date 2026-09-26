namespace StoreOps.Api.Modules.Alerts;

using StoreOps.Api.Modules.Alerts.Models;

public sealed class InMemoryAlertRepository : IAlertRepository
{
    private readonly List<Notification> _store = new();
    private readonly object _lock = new();

    public Task<Notification> AddAsync(Notification notification)
    {
        lock (_lock)
        {
            _store.Add(notification);
            return Task.FromResult(notification);
        }
    }

    public Task<List<Notification>> ListForUserAsync(Guid userId)
    {
        lock (_lock)
        {
            return Task.FromResult(_store.Where(n => n.UserId == userId).ToList());
        }
    }
}
