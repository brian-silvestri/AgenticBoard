using AgenticBoard.Domain.Enums;

namespace AgenticBoard.Application.Features.Tasks.Dtos;

public record CreateTaskDto(
    string Title,
    string? Description,
    TaskPriority Priority = TaskPriority.Medium,
    int? AssignedUserId = null
);

public record UpdateTaskDto(
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    int? AssignedUserId
);

public record UpdateTaskStatusDto(
    TaskItemStatus Status
);

public record UpdateTaskAssignmentDto(
    int? AssignedUserId
);

public record TaskItemDto(
    int Id,
    int ProjectId,
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    int? AssignedUserId,
    string? AssignedUserName,
    int CreatedById,
    string CreatedByName,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    int CommentCount
);

public record TaskCommentDto(
    int Id,
    int TaskItemId,
    int AuthorId,
    string AuthorName,
    string Text,
    DateTime CreatedAt
);

public record TaskItemDetailDto(
    int Id,
    int ProjectId,
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    int? AssignedUserId,
    string? AssignedUserName,
    int CreatedById,
    string CreatedByName,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<TaskCommentDto> Comments
);
