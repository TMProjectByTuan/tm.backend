namespace tm.Domain.Entities;

public enum InvitationStatus
{
    Pending = 1,
    Accepted = 2,
    Declined = 3,
    Expired = 4
}

public class Invitation : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Guid InvitedByUserId { get; set; }
    public string InvitedEmail { get; set; } = string.Empty;
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public DateTime ExpiresAt { get; set; }
    public Guid? AcceptedByUserId { get; set; }
    
    // Navigation properties
    public Project? Project { get; set; }
    public User? InvitedByUser { get; set; }
    public User? AcceptedByUser { get; set; }
}

