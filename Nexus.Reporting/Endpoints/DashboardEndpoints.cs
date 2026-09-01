using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nexus.Reporting.Application;
using Nexus.Reporting.Permissions;
using NexusCore.Application.Common;
using NexusCore.SharedKernel.Interfaces;

namespace Nexus.Reporting.Endpoints;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reporting").WithTags("Reporting").RequireAuthorization(ReportingPermissions.View);

        group.MapGet("/summary", async (
                Guid tenantId, Guid? organizationUnitId,
                IAuthorizationService authorizationService, HttpContext httpContext,
                IDashboardService service, CancellationToken cancellationToken) =>
            {
                var viewAll = await authorizationService.AuthorizeAsync(httpContext.User, ReportingPermissions.ViewAll);
                if (!viewAll.Succeeded)
                {
                    return Results.Forbid();
                }

                return (await service.GetSummaryAsync(tenantId, organizationUnitId, cancellationToken)).ToApiResult();
            });

        group.MapGet("/me", async (
                Guid tenantId, ICurrentUserContext currentUser,
                IDashboardService service, CancellationToken cancellationToken) =>
            {
                if (currentUser.UserId is null)
                {
                    return Results.Unauthorized();
                }

                return (await service.GetMyDashboardAsync(tenantId, currentUser.UserId.Value, cancellationToken)).ToApiResult();
            });

        group.MapGet("/projects/{projectId:guid}", async (
                Guid projectId,
                IAuthorizationService authorizationService, HttpContext httpContext,
                IDashboardService service, CancellationToken cancellationToken) =>
            {
                var viewAll = await authorizationService.AuthorizeAsync(httpContext.User, ReportingPermissions.ViewAll);
                if (!viewAll.Succeeded)
                {
                    return Results.Forbid();
                }

                return (await service.GetProjectDashboardAsync(projectId, cancellationToken)).ToApiResult();
            });

        return app;
    }
}
