namespace StoreOps.Api.Modules.Programmes.Models;

public enum ProjectRole { StoreManager, DepartmentLead, Associate }
public enum ProjectStatus { Planned, Active, Closed }

public sealed class Project
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; set; }
    public string? Description { get; set; }
    public Guid StoreId { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Planned;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

public sealed class ProjectMember
{
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public ProjectRole Role { get; set; }
}
