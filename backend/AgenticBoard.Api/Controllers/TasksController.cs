using AgenticBoard.Application.Features.Tasks.Dtos;
using AgenticBoard.Application.Features.Tasks.Services;
using AgenticBoard.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgenticBoard.Api.Controllers;

[Authorize]
public class TasksController : BaseApiController
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>
    /// List all tasks in a project with optional status and assignee filtering
    /// </summary>
    [HttpGet("/api/projects/{projectId:int}/tasks")]
    [ProducesResponseType(typeof(List<TaskItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<TaskItemDto>>> GetTasksByProject(
        int projectId,
        [FromQuery] TaskItemStatus? status,
        [FromQuery] int? assignedUserId,
        CancellationToken cancellationToken)
    {
        var tasks = await _taskService.GetTasksByProjectAsync(projectId, status, assignedUserId, cancellationToken);
        return Ok(tasks);
    }

    /// <summary>
    /// Create a new task within a project (caller must be project member)
    /// </summary>
    [HttpPost("/api/projects/{projectId:int}/tasks")]
    [ProducesResponseType(typeof(TaskItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TaskItemDto>> CreateTask(
        int projectId,
        [FromBody] CreateTaskDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _taskService.CreateTaskAsync(projectId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetTaskById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Get task details by ID including comments (caller must be member of task's project)
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TaskItemDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskItemDetailDto>> GetTaskById(int id, CancellationToken cancellationToken)
    {
        var task = await _taskService.GetTaskByIdAsync(id, cancellationToken);
        return Ok(task);
    }

    /// <summary>
    /// Update task title, description, status, priority, and assigned user
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TaskItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskItemDto>> UpdateTask(
        int id,
        [FromBody] UpdateTaskDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _taskService.UpdateTaskAsync(id, dto, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Transition task status (used by Kanban board drag & drop)
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(TaskItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskItemDto>> UpdateTaskStatus(
        int id,
        [FromBody] UpdateTaskStatusDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _taskService.UpdateTaskStatusAsync(id, dto, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Reassign task to another project member or unassign
    /// </summary>
    [HttpPatch("{id:int}/assignment")]
    [ProducesResponseType(typeof(TaskItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskItemDto>> UpdateTaskAssignment(
        int id,
        [FromBody] UpdateTaskAssignmentDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _taskService.UpdateTaskAssignmentAsync(id, dto, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Delete task
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(int id, CancellationToken cancellationToken)
    {
        await _taskService.DeleteTaskAsync(id, cancellationToken);
        return NoContent();
    }
}
