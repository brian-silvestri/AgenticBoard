using System.Text.Json.Serialization;

namespace AgenticBoard.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaskPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}
