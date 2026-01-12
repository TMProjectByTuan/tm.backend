namespace tm.Domain.Entities;

public enum SubscriptionStatus
{
    Active = 1,
    Expired = 2,
    Cancelled = 3
}

public class Subscription : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    
    // Navigation properties
    public Project? Project { get; set; }
    public User? User { get; set; }
}

