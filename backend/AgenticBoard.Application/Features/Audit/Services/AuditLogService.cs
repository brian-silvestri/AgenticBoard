using AgenticBoard.Application.Common.Exceptions;
using AgenticBoard.Application.Common.Interfaces;
using AgenticBoard.Application.Features.Audit.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AgenticBoard.Application.Features.Audit.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AuditLogService(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<AuditLogDto>> GetProjectActivityAsync(int projectId, int take = 50, CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedException("User must be authenticated.");
        }

        var currentUserId = _currentUserService.UserId.Value;

        var isMember = await _context.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == currentUserId, cancellationToken);

        if (!isMember)
        {
            throw new ForbiddenException("You do not have access to this project's activity log.");
        }

        var logs = await _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.ProjectId == projectId)
            .OrderByDescending(a => a.Timestamp)
            .Take(take)
            .Select(a => new AuditLogDto(
                a.Id,
                a.ProjectId,
                a.EntityType,
                a.EntityId,
                a.Action,
                a.OldValue,
                a.NewValue,
                a.PerformedById,
                a.PerformedByName ?? (a.PerformedBy != null ? a.PerformedBy.FullName : "System"),
                a.Timestamp
            ))
            .ToListAsync(cancellationToken);

        return logs;
    }
}
