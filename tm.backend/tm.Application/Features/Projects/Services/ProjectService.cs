using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TaskEntity = tm.Domain.Entities.Task;
using tm.Application.Common.Interfaces;
using tm.Application.Features.Email.Interfaces;
using tm.Application.Features.Projects.DTOs;
using tm.Application.Features.Projects.Interfaces;
using tm.Domain.Entities;

namespace tm.Application.Features.Projects.Services;

public class ProjectService : IProjectService
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public ProjectService(IApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        var project = new Project
        {
            Name = request.Name,
            Description = request.Description,
            CreatedByUserId = userId
        };

        _context.Projects.Add(project);

        // Add creator as leader
        var projectMember = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = userId,
            Role = ProjectRole.Leader
        };

        _context.ProjectMembers.Add(projectMember);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetProjectByIdAsync(project.Id, cancellationToken);
    }

    public async Task<ProjectResponse> GetProjectByIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects
            .Include(p => p.Members)
                .ThenInclude(pm => pm.User)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project == null)
        {
            throw new InvalidOperationException("Project not found");
        }

        return new ProjectResponse
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            CreatedByUserId = project.CreatedByUserId,
            CreatedAt = project.CreatedAt,
            Members = project.Members.Select(pm => new ProjectMemberDto
            {
                UserId = pm.UserId,
                FullName = pm.User?.FullName ?? string.Empty,
                Email = pm.User?.Email ?? string.Empty,
                Role = pm.Role.ToString()
            }).ToList()
        };
    }

    public async System.Threading.Tasks.Task InviteMemberAsync(InviteMemberRequest request, Guid leaderUserId, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
        {
            throw new InvalidOperationException("Project not found");
        }

        // Check if user is leader
        var leader = project.Members.FirstOrDefault(pm => pm.UserId == leaderUserId && pm.Role == ProjectRole.Leader);
        if (leader == null)
        {
            throw new UnauthorizedAccessException("Only project leader can invite members");
        }

        // Check if already a member (by email)
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (existingUser != null && project.Members.Any(pm => pm.UserId == existingUser.Id))
        {
            throw new InvalidOperationException("User is already a member of this project");
        }

        // Check if there's already a pending invitation for this email
        var existingInvitation = await _context.Invitations
            .FirstOrDefaultAsync(i => i.ProjectId == request.ProjectId && 
                                      i.InvitedEmail == request.Email && 
                                      i.Status == InvitationStatus.Pending &&
                                      i.ExpiresAt > DateTime.UtcNow, cancellationToken);

        if (existingInvitation != null)
        {
            throw new InvalidOperationException("An invitation has already been sent to this email");
        }

        // Create invitation (expires in 7 days)
        var invitation = new Invitation
        {
            ProjectId = project.Id,
            InvitedByUserId = leaderUserId,
            InvitedEmail = request.Email,
            Status = InvitationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _context.Invitations.Add(invitation);
        await _context.SaveChangesAsync(cancellationToken);

        // Send invitation email with link (fire and forget - không block response)
        var leaderUser = await _context.Users.FindAsync(new object[] { leaderUserId }, cancellationToken);
        if (leaderUser != null)
        {
            // Encode invitation ID to token (base64)
            var invitationToken = Convert.ToBase64String(invitation.Id.ToByteArray());
            _ = System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendProjectInvitationAsync(
                        request.Email,
                        leaderUser.FullName,
                        project.Name,
                        invitationToken,
                        cancellationToken);
                }
                catch
                {
                    // Log error nhưng không throw để không ảnh hưởng đến quá trình mời thành viên
                }
            });
        }
    }

    public async System.Threading.Tasks.Task TransferLeadershipAsync(TransferLeadershipRequest request, Guid currentLeaderUserId, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
        {
            throw new InvalidOperationException("Project not found");
        }

        var currentLeader = project.Members.FirstOrDefault(pm => pm.UserId == currentLeaderUserId && pm.Role == ProjectRole.Leader);
        if (currentLeader == null)
        {
            throw new UnauthorizedAccessException("Only project leader can transfer leadership");
        }

        var newLeader = project.Members.FirstOrDefault(pm => pm.UserId == request.NewLeaderUserId);
        if (newLeader == null)
        {
            throw new InvalidOperationException("New leader is not a member of this project");
        }

        // Transfer leadership
        currentLeader.Role = ProjectRole.Member;
        newLeader.Role = ProjectRole.Leader;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<ProjectResponse>> GetUserProjectsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var projectIds = await _context.ProjectMembers
            .Where(pm => pm.UserId == userId)
            .Select(pm => pm.ProjectId)
            .ToListAsync(cancellationToken);

        var projects = await _context.Projects
            .Include(p => p.Members)
                .ThenInclude(pm => pm.User)
            .Where(p => projectIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        return projects.Select(p => new ProjectResponse
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            CreatedByUserId = p.CreatedByUserId,
            CreatedAt = p.CreatedAt,
            Members = p.Members.Select(pm => new ProjectMemberDto
            {
                UserId = pm.UserId,
                FullName = pm.User?.FullName ?? string.Empty,
                Email = pm.User?.Email ?? string.Empty,
                Role = pm.Role.ToString()
            }).ToList()
        }).ToList();
    }
}

