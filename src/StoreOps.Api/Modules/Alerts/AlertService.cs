namespace StoreOps.Api.Modules.Alerts;

using StoreOps.Api.Modules.Alerts.Models;

public sealed class AlertService : IAlertService
{
    private readonly IAlertRepository _repository;

    public AlertService(IAlertRepository repository) => _repository = repository;

    public Task<List<Notification>> GetForUserAsync(Guid userId) => _repository.ListForUserAsync(userId);
}
