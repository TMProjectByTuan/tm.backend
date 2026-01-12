namespace tm.Domain.Entities;

public enum TaskStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Overdue = 4
}

public class Task : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Guid AssignedToUserId { get; set; }
    public Guid AssignedByUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
    public DateTime Deadline { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    // Navigation properties
    public Project? Project { get; set; }
    public User? AssignedToUser { get; set; }
    public User? AssignedByUser { get; set; }
}

