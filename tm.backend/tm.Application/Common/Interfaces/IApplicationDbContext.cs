using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TaskEntity = tm.Domain.Entities.Task;
using tm.Domain.Entities;

namespace tm.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Project> Projects { get; }
    DbSet<ProjectMember> ProjectMembers { get; }
    DbSet<TaskEntity> Tasks { get; }
    DbSet<Subscription> Subscriptions { get; }
    DbSet<Invitation> Invitations { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

