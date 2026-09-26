namespace StoreOps.Api.Modules.Programmes;

using StoreOps.Api.Modules.Programmes.Models;

public interface IProgrammeRepository
{
    Task<Project?> GetByIdAsync(Guid id);
    Task<List<Project>> ListByStoreAsync(Guid storeId);
    Task<Project> AddAsync(Project project);
    Task AddMemberAsync(ProjectMember member);
    Task<List<ProjectMember>> ListMembersAsync(Guid projectId);
}
