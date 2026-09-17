using AgenticBoard.Domain.Enums;

namespace AgenticBoard.Application.Features.Projects.Dtos;

public record ProjectMemberDto(
    int UserId,
    string Email,
    string FullName,
    ProjectRole Role,
    DateTime JoinedAt
);

public record AddProjectMemberDto(
    string Email,
    ProjectRole Role = ProjectRole.Member
);

public record ProjectDto(
    int Id,
    string Name,
    string? Description,
    int CreatedById,
    string CreatedByName,
    DateTime CreatedAt,
    ProjectRole MyRole,
    int MemberCount,
    int TaskCount
);

public record ProjectDetailDto(
    int Id,
    string Name,
    string? Description,
    int CreatedById,
    string CreatedByName,
    DateTime CreatedAt,
    ProjectRole MyRole,
    IReadOnlyList<ProjectMemberDto> Members
);
