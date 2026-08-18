namespace NexusCore.SampleTasks.Api.Tasks;

public static class TaskEndpoints
{
    public static IEndpointRouteBuilder MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sample/tasks")
            .WithTags("Sample Task Management")
            .RequireAuthorization();

        group.MapGet("/", async (TaskItemStatus? status, TaskService service, CancellationToken cancellationToken) =>
                await service.ListAsync(status, cancellationToken))
            .RequireAuthorization(TaskPermissions.View);

        group.MapGet("/{taskId:guid}", async (Guid taskId, TaskService service, CancellationToken cancellationToken) =>
                await service.GetAsync(taskId, cancellationToken))
            .RequireAuthorization(TaskPermissions.View);

        group.MapPost("/", async (CreateTaskRequest request, TaskService service, CancellationToken cancellationToken) =>
                await service.CreateAsync(request, cancellationToken))
            .RequireAuthorization(TaskPermissions.Create);

        group.MapPut("/{taskId:guid}", async (Guid taskId, UpdateTaskRequest request, TaskService service, CancellationToken cancellationToken) =>
                await service.UpdateAsync(taskId, request, cancellationToken))
            .RequireAuthorization(TaskPermissions.Update);

        group.MapPatch("/{taskId:guid}/status", async (Guid taskId, ChangeTaskStatusRequest request, TaskService service, CancellationToken cancellationToken) =>
                await service.ChangeStatusAsync(taskId, request, cancellationToken))
            .RequireAuthorization(TaskPermissions.Update);

        group.MapDelete("/{taskId:guid}", async (Guid taskId, TaskService service, CancellationToken cancellationToken) =>
                await service.DeleteAsync(taskId, cancellationToken))
            .RequireAuthorization(TaskPermissions.Delete);

        return app;
    }
}
