namespace NexusCore.SampleTasks.Api.Tasks;

public static class TaskPermissions
{
    public const string View = "tasks.view";
    public const string Create = "tasks.create";
    public const string Update = "tasks.update";
    public const string Delete = "tasks.delete";

    public static IReadOnlyList<TaskPermissionDefinition> All { get; } =
    [
        new(View, "Tasks", "View sample tasks"),
        new(Create, "Tasks", "Create sample tasks"),
        new(Update, "Tasks", "Update sample tasks"),
        new(Delete, "Tasks", "Delete sample tasks")
    ];
}

public sealed record TaskPermissionDefinition(string Name, string Module, string Description);
