using AgenticBoard.Application.Features.Audit.Dtos;
using AgenticBoard.Application.Features.Audit.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgenticBoard.Api.Controllers;

[Authorize]
public class ActivityController : BaseApiController
{
    private readonly IAuditLogService _auditLogService;

    public ActivityController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    /// <summary>
    /// Retrieve chronological activity trail for a project (ordered newest first)
    /// </summary>
    [HttpGet("/api/projects/{projectId:int}/activity")]
    [ProducesResponseType(typeof(List<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<AuditLogDto>>> GetProjectActivity(
        int projectId,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var logs = await _auditLogService.GetProjectActivityAsync(projectId, limit, cancellationToken);
        return Ok(logs);
    }
}
