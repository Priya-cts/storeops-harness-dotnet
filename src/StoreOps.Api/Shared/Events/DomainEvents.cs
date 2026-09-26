namespace StoreOps.Api.Shared.Events;

/// <summary>
/// Shared cross-module event contracts. These are the only types Alerts and Reports
/// are allowed to know about from Activities/Programmes — never the module's internal
/// Models or Repository types (that would violate the module boundary rule).
/// </summary>
public sealed record ActivityBulkStatusChangedEvent(
    Guid ActivityId,
    string PreviousStatus,
    string NewStatus,
    Guid ActorId,
    string? Note,
    DateTime Timestamp);

public sealed record ActivityCreatedEvent(Guid ActivityId, Guid ProgrammeId);

public sealed record ActivityCompletedEvent(Guid ActivityId, Guid ProgrammeId);
