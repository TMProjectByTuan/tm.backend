using tm.Application.Features.Tasks.DTOs;

namespace tm.Application.Features.Tasks.Interfaces;

public interface ITaskService
{
    Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request, Guid assignedByUserId, CancellationToken cancellationToken = default);
    Task<TaskResponse> SubmitTaskAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default);
    Task<TaskActivityResponse> GetProjectTaskActivityAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<List<TaskResponse>> GetUserTasksAsync(Guid userId, CancellationToken cancellationToken = default);
}

