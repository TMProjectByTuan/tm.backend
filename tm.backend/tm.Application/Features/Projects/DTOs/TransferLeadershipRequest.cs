namespace tm.Application.Features.Projects.DTOs;

public class TransferLeadershipRequest
{
    public Guid ProjectId { get; set; }
    public Guid NewLeaderUserId { get; set; }
}

