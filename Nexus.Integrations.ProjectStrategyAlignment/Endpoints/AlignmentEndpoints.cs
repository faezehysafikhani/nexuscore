using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nexus.Integrations.StrategyAlignment.Application;
using Nexus.Integrations.StrategyAlignment.Application.Dtos;
using Nexus.Integrations.StrategyAlignment.Permissions;
using NexusCore.Application.Common;

namespace Nexus.Integrations.StrategyAlignment.Endpoints;

public static class AlignmentEndpoints
{
    public static IEndpointRouteBuilder MapProjectStrategyAlignmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/integrations/project-strategy-alignment").WithTags("Project-Strategy Alignment").RequireAuthorization();

        group.MapGet("/", async (Guid tenantId, Guid? projectId, Guid? strategyId, IAlignmentService service, CancellationToken cancellationToken) =>
                (await service.ListAsync(tenantId, projectId, strategyId, cancellationToken)).ToApiResult())
            .RequireAuthorization(AlignmentPermissions.View);

        group.MapPost("/", async (CreateAlignmentRequest request, IAlignmentService service, CancellationToken cancellationToken) =>
                (await service.CreateAsync(request, cancellationToken)).ToApiResult())
            .RequireAuthorization(AlignmentPermissions.Manage);

        group.MapPut("/{id:guid}", async (Guid id, UpdateAlignmentRequest request, IAlignmentService service, CancellationToken cancellationToken) =>
                (await service.UpdateAsync(id, request, cancellationToken)).ToApiResult())
            .RequireAuthorization(AlignmentPermissions.Manage);

        return app;
    }
}
