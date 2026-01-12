using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TaskEntity = tm.Domain.Entities.Task;
using tm.Application.Common.Interfaces;
using tm.Application.Features.Subscriptions.DTOs;
using tm.Application.Features.Subscriptions.Interfaces;
using tm.Domain.Entities;

namespace tm.Application.Features.Subscriptions.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly IApplicationDbContext _context;

    public SubscriptionService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task CreateSubscriptionAsync(CreateSubscriptionRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId);

        if (project == null)
        {
            throw new InvalidOperationException("Project not found");
        }

        // Check if user is leader
        var leader = project.Members.FirstOrDefault(pm => pm.UserId == userId && pm.Role == ProjectRole.Leader);
        if (leader == null)
        {
            throw new UnauthorizedAccessException("Only project leader can create subscription");
        }

        // Check if subscription already exists
        var existingSubscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.ProjectId == request.ProjectId && s.Status == SubscriptionStatus.Active);

        if (existingSubscription != null)
        {
            throw new InvalidOperationException("Project already has an active subscription");
        }

        var subscription = new Subscription
        {
            ProjectId = request.ProjectId,
            UserId = userId,
            PackageName = request.PackageName,
            Price = request.Price,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(request.DurationMonths),
            Status = SubscriptionStatus.Active
        };

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

