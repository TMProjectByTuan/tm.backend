namespace tm.Application.Features.Tasks.DTOs;

public class TaskActivityResponse
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int OverdueTasks { get; set; }
    public double CompletionPercentage { get; set; }
    public List<TaskResponse> Tasks { get; set; } = new();
}

