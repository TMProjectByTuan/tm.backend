namespace tm.Application.Features.Email.Interfaces;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string toEmail, string fullName, CancellationToken cancellationToken = default);
    Task SendProjectInvitationAsync(string toEmail, string inviterName, string projectName, string invitationToken, CancellationToken cancellationToken = default);
}

