using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nexus.Actions.Application;
using Nexus.Actions.Application.Dtos;
using Nexus.Actions.Permissions;
using NexusCore.Application.Common;

namespace Nexus.Actions.Endpoints;

public static class ActionEndpoints
{
    public static IEndpointRouteBuilder MapActionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/actions").WithTags("Actions").RequireAuthorization();

        group.MapGet("/", async (Guid tenantId, Guid? projectId, IActionItemService service, CancellationToken cancellationToken) =>
                (await service.ListAsync(tenantId, projectId, cancellationToken)).ToApiResult())
            .RequireAuthorization(ActionPermissions.View);

        group.MapGet("/{id:guid}", async (Guid id, IActionItemService service, CancellationToken cancellationToken) =>
                (await service.GetAsync(id, cancellationToken)).ToApiResult())
            .RequireAuthorization(ActionPermissions.View);

        group.MapPost("/", async (CreateActionItemRequest request, IActionItemService service, CancellationToken cancellationToken) =>
                (await service.CreateAsync(request, cancellationToken)).ToApiResult())
            .RequireAuthorization(ActionPermissions.Create);

        group.MapPut("/{id:guid}", async (Guid id, UpdateActionItemRequest request, IActionItemService service, CancellationToken cancellationToken) =>
                (await service.UpdateAsync(id, request, cancellationToken)).ToApiResult())
            .RequireAuthorization(ActionPermissions.Edit);

        group.MapPut("/{id:guid}/status", async (Guid id, ChangeActionStatusRequest request, IActionItemService service, CancellationToken cancellationToken) =>
                (await service.ChangeStatusAsync(id, request, cancellationToken)).ToApiResult())
            .RequireAuthorization(ActionPermissions.Edit);

        group.MapPost("/{id:guid}/submit-for-approval", async (Guid id, IActionItemService service, CancellationToken cancellationToken) =>
                (await service.SubmitForApprovalAsync(id, cancellationToken)).ToApiResult())
            .RequireAuthorization(ActionPermissions.Submit);

        return app;
    }
}
