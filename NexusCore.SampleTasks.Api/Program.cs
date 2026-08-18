using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NexusCore.Application;
using NexusCore.Application.Identity.Dtos;
using NexusCore.Application.Identity.Interfaces;
using NexusCore.Application.Identity.Permissions;
using NexusCore.Infrastructure;
using NexusCore.Infrastructure.Persistence;
using NexusCore.Infrastructure.Security;
using NexusCore.SampleTasks.Api.Tasks;
using NexusCore.SharedKernel.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<SampleCurrentUserContext>();
builder.Services.AddScoped<ICurrentUserContext>(provider => provider.GetRequiredService<SampleCurrentUserContext>());
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSampleTaskModule(builder.Configuration);
builder.Services.AddSingleton<IAuthorizationHandler, SamplePermissionAuthorizationHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NexusCore Sample Tasks API",
        Version = "v1",
        Description = "A small test project that uses NexusCore identity, JWT, roles, and permissions, with its own task module."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            []
        }
    });
});

var jwtOptions = new JwtOptions
{
    Issuer = builder.Configuration["Jwt:Issuer"] ?? "NexusCore",
    Audience = builder.Configuration["Jwt:Audience"] ?? "NexusCore",
    SigningKey = builder.Configuration["Jwt:SigningKey"] ?? "replace-this-development-secret-with-a-production-secret-32chars",
    AccessTokenMinutes = int.TryParse(builder.Configuration["Jwt:AccessTokenMinutes"], out var minutes) ? minutes : 30
};

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options =>
{
    foreach (var permission in TaskPermissions.All)
    {
        options.AddPolicy(permission.Name, policy =>
            policy.RequireAuthenticatedUser().AddRequirements(new SamplePermissionRequirement(permission.Name)));
    }
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

var auth = app.MapGroup("/api/auth").WithTags("Authentication");
auth.MapPost("/login", async (LoginRequest request, IIdentityService identityService, CancellationToken cancellationToken) =>
{
    var result = await identityService.LoginAsync(request, cancellationToken);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.Problem(result.Error.Message, statusCode: 401);
}).AllowAnonymous();
var users = app.MapGroup("/api/identity/users").WithTags("Users").RequireAuthorization();

users.MapGet("/", async (Guid? tenantId, int pageNumber, int pageSize, string? search, IIdentityService identityService, CancellationToken cancellationToken) =>
        (await identityService.ListUsersAsync(tenantId, pageNumber, pageSize, search, cancellationToken)))
    .RequireAuthorization(IdentityPermissions.UsersView);

users.MapPost("/", async (CreateUserRequest request, IIdentityService identityService, CancellationToken cancellationToken) =>
        (await identityService.CreateUserAsync(request, cancellationToken)))
    .RequireAuthorization(IdentityPermissions.UsersCreate);

users.MapPut("/{userId:guid}", async (Guid userId, UpdateUserRequest request, IIdentityService identityService, CancellationToken cancellationToken) =>
        (await identityService.UpdateUserAsync(userId, request, cancellationToken)))
    .RequireAuthorization(IdentityPermissions.UsersUpdate);

users.MapPut("/{userId:guid}/roles", async (Guid userId, AssignUserRolesRequest request, IIdentityService identityService, CancellationToken cancellationToken) =>
        (await identityService.AssignRolesAsync(userId, request, cancellationToken)))
    .RequireAuthorization(IdentityPermissions.UsersAssignRoles);

auth.MapPost("/refresh", async (RefreshTokenRequest request, IIdentityService identityService, CancellationToken cancellationToken) =>
{
    var result = await identityService.RefreshTokenAsync(request, cancellationToken);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.Problem(result.Error.Message, statusCode: 401);
}).AllowAnonymous();

auth.MapGet("/me", async (SampleCurrentUserContext currentUser, IIdentityService identityService, CancellationToken cancellationToken) =>
{
    if (currentUser.UserId is null)
    {
        return Results.Unauthorized();
    }

    var result = await identityService.GetCurrentUserAsync(currentUser.UserId.Value, cancellationToken);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.Problem(result.Error.Message, statusCode: 404);
}).RequireAuthorization();

app.MapTaskEndpoints();

if (builder.Configuration.GetValue("Database:SeedOnStartup", true))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<DefaultDataSeeder>().SeedAsync();
    await scope.ServiceProvider.GetRequiredService<SampleTaskSeeder>().SeedAsync();
}

app.Run();

public sealed class SampleCurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    private HttpContext? HttpContext => httpContextAccessor.HttpContext;

    public Guid? UserId => Guid.TryParse(HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var userId) ? userId : null;
    public Guid? TenantId => Guid.TryParse(HttpContext?.User.FindFirst("tenant_id")?.Value, out var tenantId) ? tenantId : null;
    public string? Email => HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
    public string? IpAddress => HttpContext?.Connection.RemoteIpAddress?.ToString();
}

public sealed class SamplePermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}

public sealed class SamplePermissionAuthorizationHandler : AuthorizationHandler<SamplePermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SamplePermissionRequirement requirement)
    {
        if (context.User.HasClaim("permission", requirement.Permission) ||
            context.User.HasClaim(ClaimTypes.Role, "Administrator"))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
