namespace StoreOps.Api.Modules.Activities.Dtos;

public sealed class CreateActivityRequest
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public Guid ProgrammeId { get; set; }
    public Guid? AssigneeId { get; set; }
    public string? Priority { get; set; }
    public string? Category { get; set; }
    public DateTime? DueDate { get; set; }
}

public sealed class UpdateActivityRequest
{
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? Category { get; set; }
    public Guid? AssigneeId { get; set; }
}

/// <summary>
/// Sprint 1 demonstration feature: PATCH /api/activities/bulk-status.
/// See PROMPT.md and .harness/reviews/sprint-1-* for the governed generation chain
/// that produced this contract and its implementation.
/// </summary>
public sealed class BulkStatusUpdateRequest
{
    public required List<Guid> ActivityIds { get; set; }
    public required string Status { get; set; } // "Done" or "Blocked" only
    public required Guid ActorId { get; set; } // outgoing shift staff member
    public string? Note { get; set; }
}

public sealed class BulkStatusUpdateResult
{
    public List<Guid> Updated { get; set; } = new();
    public List<BulkStatusFailure> Failed { get; set; } = new();
}

public sealed class BulkStatusFailure
{
    public Guid ActivityId { get; set; }
    public required string Reason { get; set; }
}
