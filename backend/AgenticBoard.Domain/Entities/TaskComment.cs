using AgenticBoard.Domain.Common;

namespace AgenticBoard.Domain.Entities;

public class TaskComment : BaseEntity
{
    public int TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;

    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public string Text { get; set; } = string.Empty;
}
