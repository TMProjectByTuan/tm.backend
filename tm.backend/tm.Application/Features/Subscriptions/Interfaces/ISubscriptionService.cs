using System.Threading.Tasks;
using tm.Application.Features.Subscriptions.DTOs;

namespace tm.Application.Features.Subscriptions.Interfaces;

public interface ISubscriptionService
{
    System.Threading.Tasks.Task CreateSubscriptionAsync(CreateSubscriptionRequest request, Guid userId, CancellationToken cancellationToken = default);
}

