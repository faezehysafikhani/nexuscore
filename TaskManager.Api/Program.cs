using NexusCore.Application;
using NexusCore.Application.Extensions;
using NexusCore.Infrastructure;
using TaskManager.Sample.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplication();

// 2. سپس سرویس‌های زیرساختی (Infrastructure) را ثبت کن
builder.Services.AddInfrastructure(builder.Configuration);

// 3. سرویس‌های خودِ Task Manager
builder.Services.AddTaskModule(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapNexusCoreAuthEndpoints();
app.MapNexusCoreManagementEndpoints();
app.MapNexusCorePlatformEndpoints();
app.UseHttpsRedirection();

app.UseAuthentication(); app.UseAuthorization();

app.MapControllers();

app.Run();
