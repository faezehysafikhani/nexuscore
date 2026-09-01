using NexusCore.SharedKernel.Domain;

namespace NexusCore.SampleTasks.Api.Tasks;

public sealed class TaskItem : AuditableEntity<Guid>
{
    private TaskItem() : base(Guid.Empty)
    {
        Title = string.Empty;
        Status = TaskItemStatus.ToDo;
        Priority = TaskPriority.Normal;
    }

    public TaskItem(Guid id, Guid tenantId, string title, string? description, Guid? assignedToUserId, DateOnly? dueDate, TaskPriority priority)
        : base(id)
    {
        TenantId = tenantId;
        Title = title.Trim();
        Description = description;
        AssignedToUserId = assignedToUserId;
        DueDate = dueDate;
        Priority = priority;
        Status = TaskItemStatus.ToDo;
    }

    public Guid TenantId { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public TaskPriority Priority { get; private set; }
    public TaskItemStatus Status { get; private set; }
    public bool IsDeleted { get; private set; }

    public void Update(string title, string? description, Guid? assignedToUserId, DateOnly? dueDate, TaskPriority priority)
    {
        Title = title.Trim();
        Description = description;
        AssignedToUserId = assignedToUserId;
        DueDate = dueDate;
        Priority = priority;
    }

    public void ChangeStatus(TaskItemStatus status) => Status = status;

    public void Delete() => IsDeleted = true;
}

public enum TaskItemStatus
{
    ToDo = 0,
    InProgress = 1,
    Done = 2,
    Cancelled = 3
}

public enum TaskPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}
