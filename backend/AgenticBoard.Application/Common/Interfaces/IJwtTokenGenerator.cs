using AgenticBoard.Domain.Entities;

namespace AgenticBoard.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
