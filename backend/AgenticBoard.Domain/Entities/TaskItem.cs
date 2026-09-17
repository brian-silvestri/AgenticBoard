using AgenticBoard.Domain.Common;
using AgenticBoard.Domain.Enums;

namespace AgenticBoard.Domain.Entities;

public class TaskItem : BaseEntity
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public TaskItemStatus Status { get; set; } = TaskItemStatus.Backlog;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public int? AssignedUserId { get; set; }
    public User? AssignedUser { get; set; }

    public int CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
}
