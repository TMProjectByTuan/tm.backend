namespace tm.Application.Features.Subscriptions.DTOs;

public class CreateSubscriptionRequest
{
    public Guid ProjectId { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationMonths { get; set; } = 1;
}

