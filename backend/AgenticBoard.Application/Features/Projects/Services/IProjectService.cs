using AgenticBoard.Application.Features.Projects.Dtos;

namespace AgenticBoard.Application.Features.Projects.Services;

public interface IProjectService
{
    Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, CancellationToken cancellationToken = default);
    Task<List<ProjectDto>> GetMyProjectsAsync(CancellationToken cancellationToken = default);
    Task<ProjectDetailDto> GetProjectByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ProjectDto> UpdateProjectAsync(int id, UpdateProjectDto dto, CancellationToken cancellationToken = default);
    Task<ProjectMemberDto> AddMemberAsync(int projectId, AddProjectMemberDto dto, CancellationToken cancellationToken = default);
    Task RemoveMemberAsync(int projectId, int userId, CancellationToken cancellationToken = default);
}
