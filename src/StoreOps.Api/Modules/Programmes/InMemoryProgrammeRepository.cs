namespace StoreOps.Api.Modules.Programmes;

using StoreOps.Api.Modules.Programmes.Models;

public sealed class InMemoryProgrammeRepository : IProgrammeRepository
{
    private readonly Dictionary<Guid, Project> _projects = new();
    private readonly List<ProjectMember> _members = new();
    private readonly object _lock = new();

    public Task<Project?> GetByIdAsync(Guid id)
    {
        lock (_lock) { _projects.TryGetValue(id, out var p); return Task.FromResult(p); }
    }

    public Task<List<Project>> ListByStoreAsync(Guid storeId)
    {
        lock (_lock) { return Task.FromResult(_projects.Values.Where(p => p.StoreId == storeId).ToList()); }
    }

    public Task<Project> AddAsync(Project project)
    {
        lock (_lock) { _projects[project.Id] = project; return Task.FromResult(project); }
    }

    public Task AddMemberAsync(ProjectMember member)
    {
        lock (_lock) { _members.Add(member); return Task.CompletedTask; }
    }

    public Task<List<ProjectMember>> ListMembersAsync(Guid projectId)
    {
        lock (_lock) { return Task.FromResult(_members.Where(m => m.ProjectId == projectId).ToList()); }
    }
}
