using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TaskEntity = tm.Domain.Entities.Task;
using tm.Application.Common.Interfaces;
using tm.Application.Features.Tasks.DTOs;
using tm.Application.Features.Tasks.Interfaces;
using tm.Domain.Entities;

namespace tm.Application.Features.Tasks.Services;

public class TaskService : ITaskService
{
    private readonly IApplicationDbContext _context;

    public TaskService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request, Guid assignedByUserId, CancellationToken cancellationToken = default)
    {
        // Check if user is leader of the project
        var project = await _context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
        {
            throw new InvalidOperationException("Project not found");
        }

        var leader = project.Members.FirstOrDefault(pm => pm.UserId == assignedByUserId && pm.Role == ProjectRole.Leader);
        if (leader == null)
        {
            throw new UnauthorizedAccessException("Only project leader can assign tasks");
        }

        // Check if assigned user is a member
        var assignedMember = project.Members.FirstOrDefault(pm => pm.UserId == request.AssignedToUserId);
        if (assignedMember == null)
        {
            throw new InvalidOperationException("Assigned user is not a member of this project");
        }

        var task = new TaskEntity
        {
            ProjectId = request.ProjectId,
            AssignedToUserId = request.AssignedToUserId,
            AssignedByUserId = assignedByUserId,
            Title = request.Title,
            Description = request.Description,
            Deadline = request.Deadline,
            Status = tm.Domain.Entities.TaskStatus.Pending
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetTaskResponseAsync(task.Id, cancellationToken);
    }

    public async Task<TaskResponse> SubmitTaskAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
    {
        var task = await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.AssignedToUser)
            .Include(t => t.AssignedByUser)
            .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);

        if (task == null)
        {
            throw new InvalidOperationException("Task not found");
        }

        if (task.AssignedToUserId != userId)
        {
            throw new UnauthorizedAccessException("You can only submit your own tasks");
        }

        task.Status = tm.Domain.Entities.TaskStatus.Completed;
        task.CompletedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetTaskResponseAsync(task.Id, cancellationToken);
    }

    public async Task<TaskActivityResponse> GetProjectTaskActivityAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects.FindAsync(new object[] { projectId }, cancellationToken);
        if (project == null)
        {
            throw new InvalidOperationException("Project not found");
        }

        var tasks = await _context.Tasks
            .Include(t => t.AssignedToUser)
            .Include(t => t.AssignedByUser)
            .Where(t => t.ProjectId == projectId)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var totalTasks = tasks.Count;
        var completedTasks = tasks.Count(t => t.Status == tm.Domain.Entities.TaskStatus.Completed);
        var pendingTasks = tasks.Count(t => t.Status == tm.Domain.Entities.TaskStatus.Pending);
        var inProgressTasks = tasks.Count(t => t.Status == tm.Domain.Entities.TaskStatus.InProgress);
        var overdueTasks = tasks.Count(t => t.Status != tm.Domain.Entities.TaskStatus.Completed && t.Deadline < now);

        // Update overdue tasks
        foreach (var task in tasks.Where(t => t.Status != tm.Domain.Entities.TaskStatus.Completed && t.Deadline < now && t.Status != tm.Domain.Entities.TaskStatus.Overdue))
        {
            task.Status = tm.Domain.Entities.TaskStatus.Overdue;
        }
        await _context.SaveChangesAsync(cancellationToken);

        var completionPercentage = totalTasks > 0 ? (double)completedTasks / totalTasks * 100 : 0;

        return new TaskActivityResponse
        {
            ProjectId = projectId,
            ProjectName = project.Name,
            TotalTasks = totalTasks,
            CompletedTasks = completedTasks,
            PendingTasks = pendingTasks,
            InProgressTasks = inProgressTasks,
            OverdueTasks = overdueTasks,
            CompletionPercentage = completionPercentage,
            Tasks = tasks.Select(t => new TaskResponse
            {
                Id = t.Id,
                ProjectId = t.ProjectId,
                ProjectName = project.Name,
                AssignedToUserId = t.AssignedToUserId,
                AssignedToUserName = t.AssignedToUser?.FullName ?? string.Empty,
                AssignedByUserId = t.AssignedByUserId,
                AssignedByUserName = t.AssignedByUser?.FullName ?? string.Empty,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status.ToString(),
                Deadline = t.Deadline,
                CompletedAt = t.CompletedAt,
                CreatedAt = t.CreatedAt
            }).ToList()
        };
    }

    public async Task<List<TaskResponse>> GetUserTasksAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tasks = await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.AssignedToUser)
            .Include(t => t.AssignedByUser)
            .Where(t => t.AssignedToUserId == userId)
            .ToListAsync(cancellationToken);

        return tasks.Select(t => new TaskResponse
        {
            Id = t.Id,
            ProjectId = t.ProjectId,
            ProjectName = t.Project?.Name ?? string.Empty,
            AssignedToUserId = t.AssignedToUserId,
            AssignedToUserName = t.AssignedToUser?.FullName ?? string.Empty,
            AssignedByUserId = t.AssignedByUserId,
            AssignedByUserName = t.AssignedByUser?.FullName ?? string.Empty,
            Title = t.Title,
            Description = t.Description,
            Status = t.Status.ToString(),
            Deadline = t.Deadline,
            CompletedAt = t.CompletedAt,
            CreatedAt = t.CreatedAt
        }).ToList();
    }

    private async Task<TaskResponse> GetTaskResponseAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var task = await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.AssignedToUser)
            .Include(t => t.AssignedByUser)
            .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);

        if (task == null)
        {
            throw new InvalidOperationException("Task not found");
        }

        return new TaskResponse
        {
            Id = task.Id,
            ProjectId = task.ProjectId,
            ProjectName = task.Project?.Name ?? string.Empty,
            AssignedToUserId = task.AssignedToUserId,
            AssignedToUserName = task.AssignedToUser?.FullName ?? string.Empty,
            AssignedByUserId = task.AssignedByUserId,
            AssignedByUserName = task.AssignedByUser?.FullName ?? string.Empty,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status.ToString(),
            Deadline = task.Deadline,
            CompletedAt = task.CompletedAt,
            CreatedAt = task.CreatedAt
        };
    }
}

