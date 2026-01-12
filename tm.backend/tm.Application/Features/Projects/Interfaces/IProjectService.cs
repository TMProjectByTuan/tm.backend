using System.Threading.Tasks;
using tm.Application.Features.Projects.DTOs;

namespace tm.Application.Features.Projects.Interfaces;

public interface IProjectService
{
    System.Threading.Tasks.Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request, Guid userId, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<ProjectResponse> GetProjectByIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task InviteMemberAsync(InviteMemberRequest request, Guid leaderUserId, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task TransferLeadershipAsync(TransferLeadershipRequest request, Guid currentLeaderUserId, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<List<ProjectResponse>> GetUserProjectsAsync(Guid userId, CancellationToken cancellationToken = default);
}

