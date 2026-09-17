using AgenticBoard.Application.Common.Interfaces;

namespace AgenticBoard.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
