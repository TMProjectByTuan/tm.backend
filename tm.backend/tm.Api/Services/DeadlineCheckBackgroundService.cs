using tm.Application.Features.Notifications.Interfaces;

namespace tm.Api.Services;

public class DeadlineCheckBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DeadlineCheckBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Check every hour

    public DeadlineCheckBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<DeadlineCheckBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailNotificationService>();
                
                await emailService.CheckAndSendDeadlineWarningsAsync(stoppingToken);
                
                _logger.LogInformation("Deadline check completed at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking deadlines");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }
}

