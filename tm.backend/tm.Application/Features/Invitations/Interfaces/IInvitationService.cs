using tm.Application.Features.Invitations.DTOs;

using System.Threading.Tasks;

namespace tm.Application.Features.Invitations.Interfaces;

public interface IInvitationService
{
    System.Threading.Tasks.Task<InvitationResponse> GetInvitationByTokenAsync(string token, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task AcceptInvitationAsync(string token, Guid userId, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task DeclineInvitationAsync(string token, Guid userId, CancellationToken cancellationToken = default);
}

