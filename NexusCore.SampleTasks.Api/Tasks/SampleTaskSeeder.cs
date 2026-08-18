using Microsoft.EntityFrameworkCore;
using NexusCore.Domain.Identity;
using NexusCore.Infrastructure.Persistence;

namespace NexusCore.SampleTasks.Api.Tasks;

public sealed class SampleTaskSeeder(SampleTasksDbContext tasksDbContext, NexusCoreDbContext identityDbContext)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await tasksDbContext.Database.EnsureCreatedAsync(cancellationToken);

        foreach (var permission in TaskPermissions.All)
        {
            if (!await identityDbContext.Permissions.AnyAsync(x => x.Name == permission.Name, cancellationToken))
            {
                await identityDbContext.Permissions.AddAsync(
                    new Permission(CreateStableGuid(permission.Name), permission.Name, permission.Module, permission.Description),
                    cancellationToken);
            }
        }

        await identityDbContext.SaveChangesAsync(cancellationToken);

        var adminRole = await identityDbContext.Roles
            .Include(role => role.Permissions)
            .SingleAsync(role => role.Id == DefaultDataSeeder.AdminRoleId, cancellationToken);

        var currentPermissionIds = adminRole.Permissions.Select(permission => permission.PermissionId).ToList();
        var taskPermissionNames = TaskPermissions.All.Select(x => x.Name).ToList();
        var taskPermissionIds = await identityDbContext.Permissions
            .Where(permission => taskPermissionNames.Contains(permission.Name))
            .Select(permission => permission.Id)
            .ToListAsync(cancellationToken);

        adminRole.SetPermissions(currentPermissionIds.Concat(taskPermissionIds));
        await identityDbContext.SaveChangesAsync(cancellationToken);
    }

    private static Guid CreateStableGuid(string value)
    {
        var bytes = System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(value));
        return new Guid(bytes);
    }
}
