using Microsoft.EntityFrameworkCore;
using Nexus.ProjectManagement.Core.Domain;
using NexusCore.SharedKernel.Interfaces;

namespace Nexus.ProjectManagement.Core.Infrastructure;

public sealed class ProjectManagementCoreDbContext(DbContextOptions<ProjectManagementCoreDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Project> Projects => Set<Project>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProjectManagementCoreDbContext).Assembly);
    }
}
