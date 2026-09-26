namespace StoreOps.Api.Modules.Alerts;

using StoreOps.Api.Modules.Alerts.Models;

public interface IAlertRepository
{
    Task<Notification> AddAsync(Notification notification);
    Task<List<Notification>> ListForUserAsync(Guid userId);
}
