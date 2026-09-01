using Microsoft.EntityFrameworkCore;
using NexusCore.SharedKernel.Domain;
using NexusCore.SharedKernel.Interfaces;

namespace NexusCore.SampleTasks.Api.Tasks;

public sealed class SampleTasksDbContext(DbContextOptions<SampleTasksDbContext> options, ICurrentUserContext currentUserContext)
    : DbContext(options)
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>(builder =>
        {
            builder.ToTable("Tasks", "sample_tasks");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(2000);
            builder.Property(x => x.Status).HasConversion<int>();
            builder.Property(x => x.Priority).HasConversion<int>();
            builder.HasIndex(x => new { x.TenantId, x.Status });
            builder.HasQueryFilter(x => !x.IsDeleted);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in ChangeTracker.Entries().Where(entry => entry.Entity is AuditableEntity<Guid>))
        {
            var entity = (AuditableEntity<Guid>)entry.Entity;
            if (entry.State == EntityState.Added)
            {
                entity.CreatedAtUtc = now;
                entity.CreatedByUserId = currentUserContext.UserId;
            }

            if (entry.State == EntityState.Modified)
            {
                entity.ModifiedAtUtc = now;
                entity.ModifiedByUserId = currentUserContext.UserId;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
