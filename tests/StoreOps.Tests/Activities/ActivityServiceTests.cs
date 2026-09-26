namespace StoreOps.Tests.Activities;

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using StoreOps.Api.Modules.Activities;
using StoreOps.Api.Modules.Activities.Dtos;
using StoreOps.Api.Modules.Activities.Models;
using StoreOps.Api.Shared.Errors;
using StoreOps.Api.Shared.Events;
using Xunit;

public sealed class ActivityServiceTests
{
    private static ActivityService CreateService()
    {
        var repository = new InMemoryActivityRepository();
        var eventBus = new InMemoryEventBus(NullLogger<InMemoryEventBus>.Instance);
        return new ActivityService(repository, eventBus, NullLogger<ActivityService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_PersistsActivity()
    {
        var service = CreateService();

        var created = await service.CreateAsync(new CreateActivityRequest
        {
            Title = "Restock shelf 4",
            ProgrammeId = Guid.NewGuid(),
            Priority = "High",
            Category = "Restocking"
        });

        created.Id.Should().NotBeEmpty();
        created.Status.Should().Be(ActivityStatus.Todo);
        created.Priority.Should().Be(ActivityPriority.High);
    }

    [Fact]
    public async Task CreateAsync_WithoutTitle_ThrowsValidationError()
    {
        var service = CreateService();

        var act = async () => await service.CreateAsync(new CreateActivityRequest
        {
            Title = "",
            ProgrammeId = Guid.NewGuid()
        });

        await act.Should().ThrowAsync<ValidationError>();
    }

    [Fact]
    public async Task GetByIdAsync_WithUnknownId_ThrowsNotFoundError()
    {
        var service = CreateService();

        var act = async () => await service.GetByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundError>();
    }

    [Fact]
    public async Task UpdateAsync_SetsStatus_AndPublishesCompletedEvent()
    {
        var service = CreateService();
        var created = await service.CreateAsync(new CreateActivityRequest
        {
            Title = "Audit backroom stock",
            ProgrammeId = Guid.NewGuid()
        });

        var updated = await service.UpdateAsync(created.Id, new UpdateActivityRequest { Status = "Done" });

        updated.Status.Should().Be(ActivityStatus.Done);
    }

    [Fact]
    public async Task BulkUpdateStatusAsync_WithMixedIds_ReturnsPartialResult()
    {
        var service = CreateService();

        var created = await service.CreateAsync(new CreateActivityRequest
        {
            Title = "Planogram reset aisle 2",
            ProgrammeId = Guid.NewGuid()
        });

        var missingId = Guid.NewGuid();

        var result = await service.BulkUpdateStatusAsync(new BulkStatusUpdateRequest
        {
            ActivityIds = new List<Guid> { created.Id, missingId },
            Status = "Done",
            ActorId = Guid.NewGuid()
        });

        result.Updated.Should().ContainSingle().Which.Should().Be(created.Id);
        result.Failed.Should().ContainSingle().Which.ActivityId.Should().Be(missingId);

        var reloaded = await service.GetByIdAsync(created.Id);
        reloaded.Status.Should().Be(ActivityStatus.Done);
    }

    [Fact]
    public async Task BulkUpdateStatusAsync_WithInvalidStatus_ThrowsValidationError()
    {
        var service = CreateService();

        var act = async () => await service.BulkUpdateStatusAsync(new BulkStatusUpdateRequest
        {
            ActivityIds = new List<Guid> { Guid.NewGuid() },
            Status = "InProgress",
            ActorId = Guid.NewGuid()
        });

        await act.Should().ThrowAsync<ValidationError>();
    }

    [Fact]
    public async Task BulkUpdateStatusAsync_WithNoIds_ThrowsValidationError()
    {
        var service = CreateService();

        var act = async () => await service.BulkUpdateStatusAsync(new BulkStatusUpdateRequest
        {
            ActivityIds = new List<Guid>(),
            Status = "Done",
            ActorId = Guid.NewGuid()
        });

        await act.Should().ThrowAsync<ValidationError>();
    }
}
