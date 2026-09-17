using System.Text.Json.Serialization;

namespace AgenticBoard.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProjectRole
{
    Owner = 1,
    Member = 2
}
