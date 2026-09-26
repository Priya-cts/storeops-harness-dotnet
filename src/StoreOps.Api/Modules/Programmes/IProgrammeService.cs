namespace StoreOps.Api.Modules.Programmes;

using StoreOps.Api.Modules.Programmes.Dtos;
using StoreOps.Api.Modules.Programmes.Models;

public interface IProgrammeService
{
    Task<List<Project>> ListAsync(Guid storeId);
    Task<Project> CreateAsync(CreateProjectRequest request);
    Task AddMemberAsync(Guid projectId, AddMemberRequest request);
}
