namespace tm.Application.Features.Projects.DTOs;

public class InviteMemberRequest
{
    public Guid ProjectId { get; set; }
    public string Email { get; set; } = string.Empty;
}

