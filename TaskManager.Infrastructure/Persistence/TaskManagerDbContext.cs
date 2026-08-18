using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NexusCore.Infrastructure.Persistence;
using NexusCore.SharedKernel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Domain;
using TaskManager.Domain.Entities;


namespace TaskManager.Infrastructure.Persistence;

public class TaskManagerDbContext : DbContext
    {
    public TaskManagerDbContext(
        DbContextOptions<TaskManagerDbContext> options)
        : base(options)
    {
    }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("tasks");

        base.OnModelCreating(modelBuilder);
    }
}

