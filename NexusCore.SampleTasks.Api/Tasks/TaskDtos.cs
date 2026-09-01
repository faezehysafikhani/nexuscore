namespace NexusCore.SampleTasks.Api.Tasks;

public sealed record TaskItemDto(
    Guid Id,
    Guid TenantId,
    string Title,
    string? Description,
    Guid? AssignedToUserId,
    DateOnly? DueDate,
    TaskPriority Priority,
    TaskItemStatus Status,
    DateTimeOffset CreatedAtUtc);

public sealed record CreateTaskRequest(
    string Title,
    string? Description,
    Guid? AssignedToUserId,
    DateOnly? DueDate,
    TaskPriority Priority = TaskPriority.Normal);

public sealed record UpdateTaskRequest(
    string Title,
    string? Description,
    Guid? AssignedToUserId,
    DateOnly? DueDate,
    TaskPriority Priority);

public sealed record ChangeTaskStatusRequest(TaskItemStatus Status);
