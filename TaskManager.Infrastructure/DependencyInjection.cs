using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Infrastructure.Persistence;
namespace TaskManager.Sample.Infrastructure; 
public static class DependencyInjection 
{ 
    public static IServiceCollection AddTaskModule(this IServiceCollection services, IConfiguration configuration)
    { 
        services.AddDbContext<TaskManagerDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")))
            ; 
        return services; 
    } 
}