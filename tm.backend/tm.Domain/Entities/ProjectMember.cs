namespace tm.Domain.Entities;

public enum ProjectRole
{
    Leader = 1,
    Member = 2
}

public class ProjectMember : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public ProjectRole Role { get; set; } = ProjectRole.Member;
    
    // Navigation properties
    public Project? Project { get; set; }
    public User? User { get; set; }
}

