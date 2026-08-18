using Microsoft.EntityFrameworkCore;

namespace NexusCore.SampleTasks.Api.Tasks;

public static class DependencyInjection
{
    public static IServiceCollection AddSampleTaskModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SampleTasksDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("SampleTasksConnection")));

        services.AddScoped<TaskService>();
        services.AddScoped<SampleTaskSeeder>();

        return services;
    }
}
