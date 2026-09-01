using Microsoft.EntityFrameworkCore;
using Nexus.Organization.Domain;
using NexusCore.SharedKernel.Interfaces;

namespace Nexus.Organization.Infrastructure;

public sealed class OrganizationDbContext(DbContextOptions<OrganizationDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<OrganizationUnit> OrganizationUnits => Set<OrganizationUnit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrganizationDbContext).Assembly);
    }
}
