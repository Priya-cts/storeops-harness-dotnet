namespace StoreOps.Api.Modules.Alerts;

using StoreOps.Api.Modules.Alerts.Models;

public interface IAlertService
{
    Task<List<Notification>> GetForUserAsync(Guid userId);
}
