using AgenticBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgenticBoard.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Project> Projects { get; }
    DbSet<ProjectMember> ProjectMembers { get; }
    DbSet<TaskItem> Tasks { get; }
    DbSet<TaskComment> TaskComments { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
