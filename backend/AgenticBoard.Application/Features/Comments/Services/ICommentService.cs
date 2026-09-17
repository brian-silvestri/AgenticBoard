using AgenticBoard.Application.Features.Comments.Dtos;

namespace AgenticBoard.Application.Features.Comments.Services;

public interface ICommentService
{
    Task<CommentDto> AddCommentAsync(int taskId, CreateCommentDto dto, CancellationToken cancellationToken = default);
    Task<List<CommentDto>> GetCommentsByTaskIdAsync(int taskId, CancellationToken cancellationToken = default);
}
