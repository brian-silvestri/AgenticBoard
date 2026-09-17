using AgenticBoard.Application.Features.Projects.Dtos;
using AgenticBoard.Application.Features.Projects.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgenticBoard.Api.Controllers;

[Authorize]
public class ProjectsController : BaseApiController
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>
    /// Retrieve all projects where the authenticated user is an owner or member
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProjectDto>>> GetMyProjects(CancellationToken cancellationToken)
    {
        var projects = await _projectService.GetMyProjectsAsync(cancellationToken);
        return Ok(projects);
    }

    /// <summary>
    /// Create a new project (caller automatically becomes Owner)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectDto dto, CancellationToken cancellationToken)
    {
        var result = await _projectService.CreateProjectAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetProjectById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Get project details, including member list (requires project membership)
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDetailDto>> GetProjectById(int id, CancellationToken cancellationToken)
    {
        var project = await _projectService.GetProjectByIdAsync(id, cancellationToken);
        return Ok(project);
    }

    /// <summary>
    /// Update project name and description (requires Owner role)
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDto>> UpdateProject(int id, [FromBody] UpdateProjectDto dto, CancellationToken cancellationToken)
    {
        var updated = await _projectService.UpdateProjectAsync(id, dto, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Add a user to project by email (requires Owner role)
    /// </summary>
    [HttpPost("{id:int}/members")]
    [ProducesResponseType(typeof(ProjectMemberDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProjectMemberDto>> AddMember(int id, [FromBody] AddProjectMemberDto dto, CancellationToken cancellationToken)
    {
        var member = await _projectService.AddMemberAsync(id, dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, member);
    }

    /// <summary>
    /// Remove a member from the project (requires Owner role)
    /// </summary>
    [HttpDelete("{id:int}/members/{userId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMember(int id, int userId, CancellationToken cancellationToken)
    {
        await _projectService.RemoveMemberAsync(id, userId, cancellationToken);
        return NoContent();
    }
}
