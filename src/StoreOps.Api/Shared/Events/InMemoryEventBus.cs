namespace StoreOps.Api.Shared.Events;

public sealed class InMemoryEventBus : IEventBus
{
    private readonly Dictionary<string, List<Action<object>>> _handlers = new();
    private readonly object _lock = new();
    private readonly ILogger<InMemoryEventBus> _logger;

    public InMemoryEventBus(ILogger<InMemoryEventBus> logger)
    {
        _logger = logger;
    }

    public void Publish(string eventType, object payload)
    {
        List<Action<object>>? handlers;
        lock (_lock)
        {
            _handlers.TryGetValue(eventType, out handlers);
        }

        _logger.LogInformation(
            "EventBus: {EventType} published to {HandlerCount} handler(s)",
            eventType, handlers?.Count ?? 0);

        if (handlers is null) return;

        foreach (var handler in handlers)
        {
            handler(payload);
        }
    }

    public void Subscribe(string eventType, Action<object> handler)
    {
        lock (_lock)
        {
            if (!_handlers.TryGetValue(eventType, out var list))
            {
                list = new List<Action<object>>();
                _handlers[eventType] = list;
            }
            list.Add(handler);
        }
    }
}
