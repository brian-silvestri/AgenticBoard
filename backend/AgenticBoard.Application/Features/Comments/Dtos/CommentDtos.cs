namespace AgenticBoard.Application.Features.Comments.Dtos;

public record CreateCommentDto(
    string Text
);

public record CommentDto(
    int Id,
    int TaskItemId,
    int AuthorId,
    string AuthorName,
    string Text,
    DateTime CreatedAt
);
