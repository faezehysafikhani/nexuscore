using Microsoft.EntityFrameworkCore;
using Nexus.ProjectManagement.Waterfall.Domain;
using NexusCore.SharedKernel.Interfaces;

namespace Nexus.ProjectManagement.Waterfall.Infrastructure;

public sealed class WaterfallDbContext(DbContextOptions<WaterfallDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Activity> Activities => Set<Activity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WaterfallDbContext).Assembly);
    }
}
