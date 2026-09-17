namespace AgenticBoard.Domain.Entities;

public class AuditLog
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;

    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    public int? PerformedById { get; set; }
    public User? PerformedBy { get; set; }
    public string? PerformedByName { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
