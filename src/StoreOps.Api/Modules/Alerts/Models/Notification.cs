namespace StoreOps.Api.Modules.Alerts.Models;

public enum NotificationChannel { InApp, Email }
public enum NotificationStatus { Pending, Sent, Read }
public enum AlertType { Inventory, SlaBreach, ShiftHandover, Escalation }

public sealed class Notification
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public AlertType Type { get; set; }
    public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public required string Message { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
