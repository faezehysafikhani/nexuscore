using Microsoft.EntityFrameworkCore;
using Nexus.ProjectManagement.RiskManagement.Domain;
using NexusCore.SharedKernel.Interfaces;

namespace Nexus.ProjectManagement.RiskManagement.Infrastructure;

public sealed class RiskManagementDbContext(DbContextOptions<RiskManagementDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Risk> Risks => Set<Risk>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RiskManagementDbContext).Assembly);
    }
}
