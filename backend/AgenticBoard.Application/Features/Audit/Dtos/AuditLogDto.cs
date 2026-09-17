namespace AgenticBoard.Application.Features.Audit.Dtos;

public record AuditLogDto(
    int Id,
    int ProjectId,
    string EntityType,
    string EntityId,
    string Action,
    string? OldValue,
    string? NewValue,
    int? PerformedById,
    string? PerformedByName,
    DateTime Timestamp
);
