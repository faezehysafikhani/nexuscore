using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nexus.Integrations.ProjectWorkflow.Application;
using Nexus.Integrations.ProjectWorkflow.Permissions;
using NexusCore.Application.Common;

namespace Nexus.Integrations.ProjectWorkflow.Endpoints;

public static class ProjectWorkflowEndpoints
{
    public static IEndpointRouteBuilder MapProjectWorkflowEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/integrations/project-workflow").WithTags("Project Workflow").RequireAuthorization();

        group.MapGet("/subject-types", () => Results.Ok(ProjectManagementSubjectTypes.All))
            .RequireAuthorization(ProjectWorkflowPermissions.Configure);

        group.MapGet("/projects/{projectId:guid}/overrides", async (Guid tenantId, Guid projectId, IProjectWorkflowConfigurationService service, CancellationToken cancellationToken) =>
                (await service.ListProjectOverridesAsync(tenantId, projectId, cancellationToken)).ToApiResult())
            .RequireAuthorization(ProjectWorkflowPermissions.Configure);

        group.MapPost("/projects/{projectId:guid}/overrides", async (Guid projectId, CreateProjectWorkflowOverrideRequest request, IProjectWorkflowConfigurationService service, CancellationToken cancellationToken) =>
            {
                var effectiveRequest = request with { ProjectId = projectId };
                return (await service.CreateProjectOverrideAsync(effectiveRequest, cancellationToken)).ToApiResult();
            })
            .RequireAuthorization(ProjectWorkflowPermissions.Configure);

        return app;
    }
}
