namespace StoreOps.Api.Modules.Alerts;

using StoreOps.Api.Modules.Alerts.Models;
using StoreOps.Api.Shared.Events;

/// <summary>
/// Wires Alerts to Activities events without either module importing the other's
/// repository or service directly — this IS the "event bus only" rule in practice.
/// Registered once at startup in Program.cs.
/// </summary>
public static class AlertEventSubscriptions
{
    public static void Register(IEventBus eventBus, IAlertRepository repository)
    {
        eventBus.Subscribe("ACTIVITY_BULK_STATUS_CHANGED", payload =>
        {
            if (payload is not ActivityBulkStatusChangedEvent evt) return;

            var notification = new Notification
            {
                UserId = evt.ActorId,
                Type = AlertType.ShiftHandover,
                Channel = NotificationChannel.InApp,
                Status = NotificationStatus.Pending,
                Message = $"Activity {evt.ActivityId} marked {evt.NewStatus} during shift handover."
            };

            repository.AddAsync(notification).GetAwaiter().GetResult();
        });
    }
}
