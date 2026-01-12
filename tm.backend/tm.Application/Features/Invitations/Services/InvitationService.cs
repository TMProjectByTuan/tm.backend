using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TaskEntity = tm.Domain.Entities.Task;
using tm.Application.Common.Interfaces;
using tm.Application.Features.Invitations.DTOs;
using tm.Application.Features.Invitations.Interfaces;
using tm.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace tm.Application.Features.Invitations.Services;

public class InvitationService : IInvitationService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<InvitationService> _logger;

    public InvitationService(IApplicationDbContext context, ILogger<InvitationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<InvitationResponse> GetInvitationByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var invitationId = DecodeToken(token);
        var invitation = await _context.Invitations
            .Include(i => i.Project)
            .Include(i => i.InvitedByUser)
            .FirstOrDefaultAsync(i => i.Id == invitationId, cancellationToken);

        if (invitation == null)
        {
            throw new InvalidOperationException("Invitation not found");
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            throw new InvalidOperationException("Invitation has already been processed");
        }

        if (invitation.ExpiresAt < DateTime.UtcNow)
        {
            invitation.Status = InvitationStatus.Expired;
            await _context.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException("Invitation has expired");
        }

        return new InvitationResponse
        {
            Id = invitation.Id,
            ProjectId = invitation.ProjectId,
            ProjectName = invitation.Project != null ? invitation.Project.Name : string.Empty,
            InvitedByUserName = invitation.InvitedByUser != null ? invitation.InvitedByUser.FullName : string.Empty,
            InvitedEmail = invitation.InvitedEmail,
            Status = invitation.Status.ToString(),
            ExpiresAt = invitation.ExpiresAt,
            CreatedAt = invitation.CreatedAt
        };
    }

    public async System.Threading.Tasks.Task AcceptInvitationAsync(string token, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Accepting invitation with token for user {UserId}", userId);
        
        var invitationId = DecodeToken(token);
        var invitation = await _context.Invitations
            .Include(i => i.Project!)
                .ThenInclude(p => p.Members)
            .FirstOrDefaultAsync(i => i.Id == invitationId, cancellationToken);

        if (invitation == null)
        {
            _logger.LogWarning("Invitation not found for token");
            throw new InvalidOperationException("Invitation not found");
        }
        
        _logger.LogInformation("Found invitation {InvitationId} for email {Email}", invitation.Id, invitation.InvitedEmail);

        if (invitation.Status != InvitationStatus.Pending)
        {
            throw new InvalidOperationException("Invitation has already been processed");
        }

        if (invitation.ExpiresAt < DateTime.UtcNow)
        {
            invitation.Status = InvitationStatus.Expired;
            await _context.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException("Invitation has expired");
        }

        // Verify user email matches invitation email (case-insensitive)
        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", userId);
            throw new UnauthorizedAccessException("User not found");
        }
        
        _logger.LogInformation("User email: {UserEmail}, Invited email: {InvitedEmail}", user.Email, invitation.InvitedEmail);
        
        if (!string.Equals(user.Email, invitation.InvitedEmail, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Email mismatch - User: {UserEmail}, Invited: {InvitedEmail}", user.Email, invitation.InvitedEmail);
            throw new UnauthorizedAccessException($"Email does not match invitation. Your email: {user.Email}, Invited email: {invitation.InvitedEmail}");
        }

        // Validate project exists
        if (invitation.Project == null)
        {
            throw new InvalidOperationException("Project not found");
        }

        // Check if already a member
        if (invitation.Project.Members.Any(pm => pm.UserId == userId))
        {
            invitation.Status = InvitationStatus.Declined;
            await _context.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException("User is already a member of this project");
        }

        // Check if project has more than 4 members (including the new one)
        var currentMemberCount = invitation.Project.Members.Count;
        if (currentMemberCount >= 4)
        {
            var hasActiveSubscription = await _context.Subscriptions
                .AnyAsync(s => s.ProjectId == invitation.ProjectId &&
                              s.Status == SubscriptionStatus.Active &&
                              s.EndDate > DateTime.UtcNow, cancellationToken);

            if (!hasActiveSubscription)
            {
                throw new InvalidOperationException("Project with more than 4 members requires an active subscription");
            }
        }

        // Add user to project
        var projectMember = new ProjectMember
        {
            ProjectId = invitation.ProjectId,
            UserId = userId,
            Role = ProjectRole.Member
        };

        _context.ProjectMembers.Add(projectMember);

        // Update invitation status
        invitation.Status = InvitationStatus.Accepted;
        invitation.AcceptedByUserId = userId;
        invitation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task DeclineInvitationAsync(string token, Guid userId, CancellationToken cancellationToken = default)
    {
        var invitationId = DecodeToken(token);
        var invitation = await _context.Invitations
            .FirstOrDefaultAsync(i => i.Id == invitationId, cancellationToken);

        if (invitation == null)
        {
            throw new InvalidOperationException("Invitation not found");
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            throw new InvalidOperationException("Invitation has already been processed");
        }

        // Verify user email matches invitation email (case-insensitive)
        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }
        
        if (!string.Equals(user.Email, invitation.InvitedEmail, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException($"Email does not match invitation. Your email: {user.Email}, Invited email: {invitation.InvitedEmail}");
        }

        invitation.Status = InvitationStatus.Declined;
        invitation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    private Guid DecodeToken(string token)
    {
        try
        {
            // Simple base64 decode - in production, use proper encryption
            var bytes = Convert.FromBase64String(token);
            return new Guid(bytes);
        }
        catch
        {
            throw new InvalidOperationException("Invalid invitation token");
        }
    }

    public static string EncodeToken(Guid invitationId)
    {
        // Simple base64 encode - in production, use proper encryption
        return Convert.ToBase64String(invitationId.ToByteArray());
    }
}

