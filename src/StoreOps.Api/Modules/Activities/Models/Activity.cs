namespace StoreOps.Api.Modules.Activities.Models;

// Note: the StoreOps spec calls this entity "Task". It is named "Activity" here
// deliberately to avoid colliding with System.Threading.Tasks.Task, which every
// async method in this codebase already uses. This naming decision is recorded
// in DESIGN_BRIEF.md, Section D (Architectural Decisions).

public enum ActivityStatus { Todo, InProgress, Done, Blocked }
public enum ActivityPriority { Low, Medium, High, Critical }
public enum ActivityCategory { Restocking, Planogram, Audit, Compliance, General }

public sealed class Activity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Title { get; set; }
    public string? Description { get; set; }
    public Guid ProgrammeId { get; set; }
    public Guid? AssigneeId { get; set; }
    public ActivityStatus Status { get; set; } = ActivityStatus.Todo;
    public ActivityPriority Priority { get; set; } = ActivityPriority.Medium;
    public ActivityCategory Category { get; set; } = ActivityCategory.General;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
