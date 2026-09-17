using System.Text.Json.Serialization;

namespace AgenticBoard.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaskItemStatus
{
    Backlog = 1,
    Todo = 2,
    InProgress = 3,
    Review = 4,
    Done = 5
}
