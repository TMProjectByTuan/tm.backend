using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using tm.Api.Helpers;
using tm.Application.Features.Auth.DTOs;
using tm.Application.Features.Auth.Interfaces;
using tm.Application.Features.Auth.Services;
using tm.Application.Features.Email.Interfaces;
using tm.Application.Features.Email.Services;
using tm.Application.Features.Notifications.Interfaces;
using tm.Application.Features.Notifications.Services;
using tm.Application.Features.Projects.DTOs;
using tm.Application.Features.Projects.Interfaces;
using tm.Application.Features.Projects.Services;
using tm.Application.Features.Subscriptions.DTOs;
using tm.Application.Features.Subscriptions.Interfaces;
using tm.Application.Features.Subscriptions.Services;
using tm.Application.Features.Tasks.DTOs;
using tm.Application.Features.Tasks.Interfaces;
using tm.Application.Features.Tasks.Services;
using tm.Application.Features.Invitations.DTOs;
using tm.Application.Features.Invitations.Interfaces;
using tm.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Infrastructure services
builder.Services.AddInfrastructureServices(builder.Configuration);

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!";
var issuer = jwtSettings["Issuer"] ?? "TMProject";
var audience = jwtSettings["Audience"] ?? "TMProjectUsers";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// Register Application Services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IEmailNotificationService, EmailNotificationService>();
builder.Services.AddScoped<tm.Application.Features.Invitations.Interfaces.IInvitationService, tm.Application.Features.Invitations.Services.InvitationService>();

// Register Background Services
builder.Services.AddHostedService<tm.Api.Services.DeadlineCheckBackgroundService>();

var app = builder.Build();

app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// ==================== AUTH ENDPOINTS ====================
app.MapPost("/api/auth/register", async (
    [FromBody] RegisterRequest request,
    IAuthService authService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var result = await authService.RegisterAsync(request, cancellationToken);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("Register")
.WithTags("Auth")
.Produces<RegisterResponse>()
.Produces(400);

app.MapPost("/api/auth/login", async (
    [FromBody] LoginRequest request,
    IAuthService authService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        return Results.Ok(result);
    }
    catch (UnauthorizedAccessException ex)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("Login")
.WithTags("Auth")
.Produces<LoginResponse>()
.Produces(401)
.Produces(400);

// ==================== PROJECT ENDPOINTS ====================
app.MapPost("/api/projects", async (
    HttpContext httpContext,
    [FromBody] CreateProjectRequest request,
    IProjectService projectService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var userId = JwtHelper.GetUserId(httpContext);
        var result = await projectService.CreateProjectAsync(request, userId, cancellationToken);
        return Results.Created($"/api/projects/{result.Id}", result);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.RequireAuthorization()
.WithName("CreateProject")
.WithTags("Projects")
.Produces<ProjectResponse>(201)
.Produces(400)
.Produces(401);

app.MapGet("/api/projects/{id:guid}", async (
    Guid id,
    IProjectService projectService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var result = await projectService.GetProjectByIdAsync(id, cancellationToken);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
})
.WithName("GetProject")
.WithTags("Projects")
.Produces<ProjectResponse>()
.Produces(404);

app.MapGet("/api/projects/my-projects", async (
    HttpContext httpContext,
    IProjectService projectService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var userId = JwtHelper.GetUserId(httpContext);
        var result = await projectService.GetUserProjectsAsync(userId, cancellationToken);
        return Results.Ok(result);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.RequireAuthorization()
.WithName("GetUserProjects")
.WithTags("Projects")
.Produces<List<ProjectResponse>>()
.Produces(400)
.Produces(401);

app.MapPost("/api/projects/invite", async (
    HttpContext httpContext,
    [FromBody] InviteMemberRequest request,
    IProjectService projectService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var leaderUserId = JwtHelper.GetUserId(httpContext);
        await projectService.InviteMemberAsync(request, leaderUserId, cancellationToken);
        return Results.Ok(new { message = "Member invited successfully" });
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.RequireAuthorization()
.WithName("InviteMember")
.WithTags("Projects")
.Produces(200)
.Produces(400)
.Produces(401);

app.MapPost("/api/projects/transfer-leadership", async (
    HttpContext httpContext,
    [FromBody] TransferLeadershipRequest request,
    IProjectService projectService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var currentLeaderUserId = JwtHelper.GetUserId(httpContext);
        await projectService.TransferLeadershipAsync(request, currentLeaderUserId, cancellationToken);
        return Results.Ok(new { message = "Leadership transferred successfully" });
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.RequireAuthorization()
.WithName("TransferLeadership")
.WithTags("Projects")
.Produces(200)
.Produces(400)
.Produces(401);

// ==================== TASK ENDPOINTS ====================
app.MapPost("/api/tasks", async (
    HttpContext httpContext,
    [FromBody] CreateTaskRequest request,
    ITaskService taskService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var assignedByUserId = JwtHelper.GetUserId(httpContext);
        var result = await taskService.CreateTaskAsync(request, assignedByUserId, cancellationToken);
        return Results.Created($"/api/tasks/{result.Id}", result);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.RequireAuthorization()
.WithName("CreateTask")
.WithTags("Tasks")
.Produces<TaskResponse>(201)
.Produces(400)
.Produces(401);

app.MapPost("/api/tasks/{id:guid}/submit", async (
    HttpContext httpContext,
    Guid id,
    ITaskService taskService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var userId = JwtHelper.GetUserId(httpContext);
        var result = await taskService.SubmitTaskAsync(id, userId, cancellationToken);
        return Results.Ok(result);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (InvalidOperationException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
})
.RequireAuthorization()
.WithName("SubmitTask")
.WithTags("Tasks")
.Produces<TaskResponse>()
.Produces(404)
.Produces(401);

app.MapGet("/api/tasks/project/{projectId:guid}/activity", async (
    Guid projectId,
    ITaskService taskService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var result = await taskService.GetProjectTaskActivityAsync(projectId, cancellationToken);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
})
.WithName("GetProjectTaskActivity")
.WithTags("Tasks")
.Produces<TaskActivityResponse>()
.Produces(404);

app.MapGet("/api/tasks/my-tasks", async (
    HttpContext httpContext,
    ITaskService taskService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var userId = JwtHelper.GetUserId(httpContext);
        var result = await taskService.GetUserTasksAsync(userId, cancellationToken);
        return Results.Ok(result);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.RequireAuthorization()
.WithName("GetUserTasks")
.WithTags("Tasks")
.Produces<List<TaskResponse>>()
.Produces(400)
.Produces(401);

// ==================== SUBSCRIPTION ENDPOINTS ====================
app.MapPost("/api/subscriptions", async (
    HttpContext httpContext,
    [FromBody] CreateSubscriptionRequest request,
    ISubscriptionService subscriptionService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var userId = JwtHelper.GetUserId(httpContext);
        await subscriptionService.CreateSubscriptionAsync(request, userId, cancellationToken);
        return Results.Created($"/api/subscriptions", new { message = "Subscription created successfully" });
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.RequireAuthorization()
.WithName("CreateSubscription")
.WithTags("Subscriptions")
.Produces(201)
.Produces(400)
.Produces(401);

// ==================== NOTIFICATION ENDPOINTS ====================
app.MapPost("/api/notifications/check-deadlines", async (
    IEmailNotificationService emailService,
    CancellationToken cancellationToken) =>
{
    try
    {
        await emailService.CheckAndSendDeadlineWarningsAsync(cancellationToken);
        return Results.Ok(new { message = "Deadline warnings checked and sent" });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("CheckDeadlines")
.WithTags("Notifications")
.Produces(200)
.Produces(400);

// ==================== INVITATION ENDPOINTS ====================
app.MapGet("/api/invitations/{token}", async (
    string token,
    IInvitationService invitationService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var result = await invitationService.GetInvitationByTokenAsync(token, cancellationToken);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("GetInvitation")
.WithTags("Invitations")
.Produces<InvitationResponse>()
.Produces(400);

app.MapPost("/api/invitations/{token}/accept", async (
    HttpContext httpContext,
    string token,
    IInvitationService invitationService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var userId = JwtHelper.GetUserId(httpContext);
        await invitationService.AcceptInvitationAsync(token, userId, cancellationToken);
        return Results.Ok(new { message = "Invitation accepted successfully" });
    }
    catch (UnauthorizedAccessException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.RequireAuthorization()
.WithName("AcceptInvitation")
.WithTags("Invitations")
.Produces(200)
.Produces(400)
.Produces(401);

app.MapPost("/api/invitations/{token}/decline", async (
    HttpContext httpContext,
    string token,
    IInvitationService invitationService,
    CancellationToken cancellationToken) =>
{
    try
    {
        var userId = JwtHelper.GetUserId(httpContext);
        await invitationService.DeclineInvitationAsync(token, userId, cancellationToken);
        return Results.Ok(new { message = "Invitation declined successfully" });
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.RequireAuthorization()
.WithName("DeclineInvitation")
.WithTags("Invitations")
.Produces(200)
.Produces(400)
.Produces(401);

app.Run();
