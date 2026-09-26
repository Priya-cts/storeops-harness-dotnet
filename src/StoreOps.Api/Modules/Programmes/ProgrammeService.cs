namespace StoreOps.Api.Modules.Programmes;

using StoreOps.Api.Modules.Programmes.Dtos;
using StoreOps.Api.Modules.Programmes.Models;
using StoreOps.Api.Shared.Errors;
using StoreOps.Api.Shared.Events;

public sealed class ProgrammeService : IProgrammeService
{
    private readonly IProgrammeRepository _repository;
    private readonly IEventBus _eventBus;

    public ProgrammeService(IProgrammeRepository repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public Task<List<Project>> ListAsync(Guid storeId) => _repository.ListByStoreAsync(storeId);

    public async Task<Project> CreateAsync(CreateProjectRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationError("Name is required.");

        var project = new Project
        {
            Name = request.Name,
            Description = request.Description,
            StoreId = request.StoreId
        };

        var created = await _repository.AddAsync(project);
        _eventBus.Publish("PROGRAMME_CREATED", new { created.Id, created.StoreId });
        return created;
    }

    public async Task AddMemberAsync(Guid projectId, AddMemberRequest request)
    {
        var project = await _repository.GetByIdAsync(projectId);
        if (project is null) throw new NotFoundError("Project", projectId);

        if (!Enum.TryParse<ProjectRole>(request.Role, true, out var role))
            throw new ValidationError($"Unknown role '{request.Role}'.");

        await _repository.AddMemberAsync(new ProjectMember
        {
            ProjectId = projectId,
            UserId = request.UserId,
            Role = role
        });
    }
}
