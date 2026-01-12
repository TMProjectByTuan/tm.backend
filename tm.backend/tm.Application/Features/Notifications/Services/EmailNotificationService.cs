using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TaskEntity = tm.Domain.Entities.Task;
using tm.Application.Common.Interfaces;
using tm.Application.Features.Notifications.Interfaces;
using tm.Domain.Entities;

namespace tm.Application.Features.Notifications.Services;

public class EmailNotificationService : IEmailNotificationService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(
        IApplicationDbContext context,
        ILogger<EmailNotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task SendDeadlineWarningAsync(
        Guid taskId, 
        string userEmail, 
        string taskTitle, 
        DateTime deadline, 
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement actual email sending (SMTP, SendGrid, etc.)
        // For now, just log the notification
        _logger.LogWarning(
            "DEADLINE WARNING: Task '{TaskTitle}' (ID: {TaskId}) assigned to {UserEmail} is approaching deadline on {Deadline}",
            taskTitle, taskId, userEmail, deadline);

        // In production, you would use an email service like:
        // - SMTP client
        // - SendGrid
        // - Azure Communication Services
        // - AWS SES
        // etc.
        
        // Example implementation would be:
        // await _emailClient.SendAsync(new EmailMessage
        // {
        //     To = userEmail,
        //     Subject = $"Deadline Warning: {taskTitle}",
        //     Body = $"Your task '{taskTitle}' is due on {deadline:dd/MM/yyyy HH:mm}. Please complete it soon."
        // });
    }

    public async System.Threading.Tasks.Task CheckAndSendDeadlineWarningsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var warningThreshold = TimeSpan.FromDays(1); // Warn 1 day before deadline

        var tasksToWarn = await _context.Tasks
            .Include(t => t.AssignedToUser)
            .Include(t => t.Project)
            .Where(t => t.Status != tm.Domain.Entities.TaskStatus.Completed &&
                       t.Deadline > now &&
                       t.Deadline <= now.Add(warningThreshold))
            .ToListAsync(cancellationToken);

        foreach (var task in tasksToWarn)
        {
            if (task.AssignedToUser != null)
            {
                await SendDeadlineWarningAsync(
                    task.Id,
                    task.AssignedToUser.Email,
                    task.Title,
                    task.Deadline,
                    cancellationToken);
            }
        }

        _logger.LogInformation("Checked {Count} tasks for deadline warnings", tasksToWarn.Count);
    }
}

