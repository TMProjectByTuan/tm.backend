namespace tm.Application.Features.Tasks.DTOs;

public class CreateTaskRequest
{
    public Guid ProjectId { get; set; }
    public Guid AssignedToUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Deadline { get; set; }
}

