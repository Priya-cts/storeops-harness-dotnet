namespace StoreOps.Api.Modules.Staff.Models;

public enum StaffRole { RegionalManager, StoreManager, DepartmentLead, Associate }

public sealed class User
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Email { get; set; }
    public Guid StoreId { get; set; }
    public StaffRole Role { get; set; } = StaffRole.Associate;
}
