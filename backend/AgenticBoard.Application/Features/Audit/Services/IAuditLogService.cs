using AgenticBoard.Application.Features.Audit.Dtos;

namespace AgenticBoard.Application.Features.Audit.Services;

public interface IAuditLogService
{
    Task<List<AuditLogDto>> GetProjectActivityAsync(int projectId, int take = 50, CancellationToken cancellationToken = default);
}
