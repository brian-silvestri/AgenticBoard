using AgenticBoard.Application.Features.Comments.Dtos;
using AgenticBoard.Application.Features.Comments.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgenticBoard.Api.Controllers;

[Authorize]
public class CommentsController : BaseApiController
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    /// <summary>
    /// Retrieve all comments for a specific task
    /// </summary>
    [HttpGet("/api/tasks/{taskId:int}/comments")]
    [ProducesResponseType(typeof(List<CommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<CommentDto>>> GetComments(int taskId, CancellationToken cancellationToken)
    {
        var comments = await _commentService.GetCommentsByTaskIdAsync(taskId, cancellationToken);
        return Ok(comments);
    }

    /// <summary>
    /// Add a new comment to a task
    /// </summary>
    [HttpPost("/api/tasks/{taskId:int}/comments")]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommentDto>> AddComment(int taskId, [FromBody] CreateCommentDto dto, CancellationToken cancellationToken)
    {
        var result = await _commentService.AddCommentAsync(taskId, dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }
}
