namespace StoreOps.Api.Modules.Activities;

using StoreOps.Api.Modules.Activities.Dtos;
using StoreOps.Api.Modules.Activities.Models;
using StoreOps.Api.Shared.Errors;
using StoreOps.Api.Shared.Events;

public sealed class ActivityService : IActivityService
{
    private readonly IActivityRepository _repository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<ActivityService> _logger;

    private static readonly HashSet<ActivityStatus> AllowedBulkStatuses = new()
    {
        ActivityStatus.Done,
        ActivityStatus.Blocked
    };

    public ActivityService(IActivityRepository repository, IEventBus eventBus, ILogger<ActivityService> logger)
    {
        _repository = repository;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<Activity> GetByIdAsync(Guid id)
    {
        var activity = await _repository.GetByIdAsync(id);
        if (activity is null) throw new NotFoundError("Activity", id);
        return activity;
    }

    public Task<List<Activity>> ListAsync(Guid? programmeId, string? status)
    {
        ActivityStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<ActivityStatus>(status, true, out var s))
                throw new ValidationError($"Unknown status '{status}'.");
            parsedStatus = s;
        }
        return _repository.ListAsync(programmeId, parsedStatus);
    }

    public async Task<Activity> CreateAsync(CreateActivityRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ValidationError("Title is required.");

        var activity = new Activity
        {
            Title = request.Title,
            Description = request.Description,
            ProgrammeId = request.ProgrammeId,
            AssigneeId = request.AssigneeId,
            Priority = ParseEnumOrDefault(request.Priority, ActivityPriority.Medium),
            Category = ParseEnumOrDefault(request.Category, ActivityCategory.General),
            DueDate = request.DueDate
        };

        var created = await _repository.AddAsync(activity);
        _eventBus.Publish("ACTIVITY_CREATED", new ActivityCreatedEvent(created.Id, created.ProgrammeId));
        return created;
    }

    public async Task<Activity> UpdateAsync(Guid id, UpdateActivityRequest request)
    {
        var activity = await GetByIdAsync(id);

        if (!string.IsNullOrWhiteSpace(request.Status))
            activity.Status = ParseEnumOrThrow<ActivityStatus>(request.Status);

        if (!string.IsNullOrWhiteSpace(request.Priority))
            activity.Priority = ParseEnumOrThrow<ActivityPriority>(request.Priority);

        if (!string.IsNullOrWhiteSpace(request.Category))
            activity.Category = ParseEnumOrThrow<ActivityCategory>(request.Category);

        if (request.AssigneeId.HasValue)
            activity.AssigneeId = request.AssigneeId;

        await _repository.UpdateAsync(activity);

        if (activity.Status == ActivityStatus.Done)
        {
            _eventBus.Publish("ACTIVITY_COMPLETED", new ActivityCompletedEvent(activity.Id, activity.ProgrammeId));
        }

        return activity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (!deleted) throw new NotFoundError("Activity", id);
    }

    /// <summary>
    /// Sprint 1 demonstration feature. Allows outgoing shift staff to mark multiple
    /// activities DONE or BLOCKED in one request. Uses partial-failure semantics
    /// (unknown ids are reported in Failed, not thrown) because a bulk shift-handover
    /// action must not lose valid updates just because one id in the batch is stale.
    /// Each successful change raises an audit event via the event bus (never a direct
    /// call into the Alerts module) per the "Event bus only" architecture rule.
    /// </summary>
    public async Task<BulkStatusUpdateResult> BulkUpdateStatusAsync(BulkStatusUpdateRequest request)
    {
        if (request.ActivityIds is null || request.ActivityIds.Count == 0)
            throw new ValidationError("At least one activityId is required.");

        if (!Enum.TryParse<ActivityStatus>(request.Status, true, out var targetStatus)
            || !AllowedBulkStatuses.Contains(targetStatus))
        {
            throw new ValidationError("Bulk status update only supports 'Done' or 'Blocked'.");
        }

        var result = new BulkStatusUpdateResult();

        foreach (var activityId in request.ActivityIds)
        {
            var activity = await _repository.GetByIdAsync(activityId);
            if (activity is null)
            {
                result.Failed.Add(new BulkStatusFailure { ActivityId = activityId, Reason = "Activity not found." });
                continue;
            }

            var previousStatus = activity.Status;
            activity.Status = targetStatus;
            await _repository.UpdateAsync(activity);

            // Audit entry per updated task — raised via the event bus, not a direct
            // cross-module import into Alerts.
            _eventBus.Publish("ACTIVITY_BULK_STATUS_CHANGED", new ActivityBulkStatusChangedEvent(
                activity.Id,
                previousStatus.ToString(),
                targetStatus.ToString(),
                request.ActorId,
                request.Note,
                DateTime.UtcNow));

            result.Updated.Add(activity.Id);
        }

        _logger.LogInformation(
            "Bulk status update by {ActorId}: {UpdatedCount} updated, {FailedCount} failed",
            request.ActorId, result.Updated.Count, result.Failed.Count);

        return result;
    }

    private static TEnum ParseEnumOrDefault<TEnum>(string? value, TEnum fallback) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value)) return fallback;
        return Enum.TryParse<TEnum>(value, true, out var parsed) ? parsed : fallback;
    }

    private static TEnum ParseEnumOrThrow<TEnum>(string value) where TEnum : struct, Enum
    {
        if (!Enum.TryParse<TEnum>(value, true, out var parsed))
            throw new ValidationError($"Unknown value '{value}' for {typeof(TEnum).Name}.");
        return parsed;
    }
}
