namespace StoreOps.Api.Shared.Events;

/// <summary>
/// Architecture rule (Section 3.5, "Event bus only"): side effects that cross module
/// boundaries must be raised through this bus, never via a direct service-to-service
/// import (e.g. Activities must not new-up or inject NotificationService directly).
/// Payloads are typed contract records defined in this Shared/Events namespace so
/// publisher and subscriber modules only depend on a shared contract, not on each
/// other's internals.
/// </summary>
public interface IEventBus
{
    void Publish(string eventType, object payload);
    void Subscribe(string eventType, Action<object> handler);
}
