using Microsoft.EntityFrameworkCore;
using Nexus.Calendar.Domain;
using NexusCore.SharedKernel.Interfaces;

namespace Nexus.Calendar.Infrastructure;

public sealed class CalendarDbContext(DbContextOptions<CalendarDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<WorkCalendar> WorkCalendars => Set<WorkCalendar>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CalendarDbContext).Assembly);
    }
}
