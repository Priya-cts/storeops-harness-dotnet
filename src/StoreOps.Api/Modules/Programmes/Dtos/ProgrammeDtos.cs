namespace StoreOps.Api.Modules.Programmes.Dtos;

public sealed class CreateProjectRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public Guid StoreId { get; set; }
}

public sealed class AddMemberRequest
{
    public Guid UserId { get; set; }
    public required string Role { get; set; }
}
