using System.Threading.Tasks;

namespace tm.Application.Features.Notifications.Interfaces;

public interface IEmailNotificationService
{
    System.Threading.Tasks.Task SendDeadlineWarningAsync(Guid taskId, string userEmail, string taskTitle, DateTime deadline, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task CheckAndSendDeadlineWarningsAsync(CancellationToken cancellationToken = default);
}

