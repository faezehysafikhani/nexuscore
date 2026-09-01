using Microsoft.EntityFrameworkCore;
using NexusCore.Application.Platform.Interfaces;
using NexusCore.SharedKernel.Interfaces;

namespace NexusCore.SampleTasks.Api.Tasks;

public sealed class TaskService(
    SampleTasksDbContext dbContext,
    ICurrentUserContext currentUserContext,
    IPlatformService platformService)
{
    public async Task<IResult> ListAsync(TaskItemStatus? status, CancellationToken cancellationToken)
    {
        if (currentUserContext.TenantId is null)
        {
            return Results.Unauthorized();
        }

        var query = dbContext.Tasks.AsNoTracking().Where(task => task.TenantId == currentUserContext.TenantId);
        if (status.HasValue)
        {
            query = query.Where(task => task.Status == status.Value);
        }

        var tasks = await query
            .OrderByDescending(task => task.CreatedAtUtc)
            .Select(task => ToDto(task))
            .ToListAsync(cancellationToken);

        return Results.Ok(tasks);
    }

    public async Task<IResult> GetAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var task = await FindTenantTaskAsync(taskId, cancellationToken);
        return task is null ? Results.NotFound() : Results.Ok(ToDto(task));
    }

    public async Task<IResult> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken)
    {
        if (currentUserContext.TenantId is null)
        {
            return Results.Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Results.BadRequest("Task title is required.");
        }

        var task = new TaskItem(
            Guid.NewGuid(),
            currentUserContext.TenantId.Value,
            request.Title,
            request.Description,
            request.AssignedToUserId,
            request.DueDate,
            request.Priority);

        await dbContext.Tasks.AddAsync(task, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await platformService.AuditAsync("tasks.create", nameof(TaskItem), task.Id.ToString(), task.Title, cancellationToken);

        return Results.Created($"/api/sample/tasks/{task.Id}", ToDto(task));
    }

    public async Task<IResult> UpdateAsync(Guid taskId, UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Results.BadRequest("Task title is required.");
        }

        var task = await FindTenantTaskAsync(taskId, cancellationToken);
        if (task is null)
        {
            return Results.NotFound();
        }

        task.Update(request.Title, request.Description, request.AssignedToUserId, request.DueDate, request.Priority);
        await dbContext.SaveChangesAsync(cancellationToken);
        await platformService.AuditAsync("tasks.update", nameof(TaskItem), task.Id.ToString(), task.Title, cancellationToken);

        return Results.Ok(ToDto(task));
    }

    public async Task<IResult> ChangeStatusAsync(Guid taskId, ChangeTaskStatusRequest request, CancellationToken cancellationToken)
    {
        var task = await FindTenantTaskAsync(taskId, cancellationToken);
        if (task is null)
        {
            return Results.NotFound();
        }

        task.ChangeStatus(request.Status);
        await dbContext.SaveChangesAsync(cancellationToken);
        await platformService.AuditAsync("tasks.change_status", nameof(TaskItem), task.Id.ToString(), request.Status.ToString(), cancellationToken);

        return Results.Ok(ToDto(task));
    }

    public async Task<IResult> DeleteAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var task = await FindTenantTaskAsync(taskId, cancellationToken);
        if (task is null)
        {
            return Results.NotFound();
        }

        task.Delete();
        await dbContext.SaveChangesAsync(cancellationToken);
        await platformService.AuditAsync("tasks.delete", nameof(TaskItem), task.Id.ToString(), task.Title, cancellationToken);

        return Results.NoContent();
    }

    private Task<TaskItem?> FindTenantTaskAsync(Guid taskId, CancellationToken cancellationToken)
    {
        if (currentUserContext.TenantId is null)
        {
            return Task.FromResult<TaskItem?>(null);
        }

        return dbContext.Tasks.SingleOrDefaultAsync(
            task => task.Id == taskId && task.TenantId == currentUserContext.TenantId,
            cancellationToken);
    }

    private static TaskItemDto ToDto(TaskItem task) =>
        new(task.Id, task.TenantId, task.Title, task.Description, task.AssignedToUserId, task.DueDate, task.Priority, task.Status, task.CreatedAtUtc);
}
