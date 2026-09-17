namespace AgenticBoard.Application.Features.Projects.Dtos;

public record CreateProjectDto(
    string Name,
    string? Description
);

public record UpdateProjectDto(
    string Name,
    string? Description
);
