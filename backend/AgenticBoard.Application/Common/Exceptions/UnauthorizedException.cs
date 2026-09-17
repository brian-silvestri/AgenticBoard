namespace AgenticBoard.Application.Common.Exceptions;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Authentication failed or token is invalid.") : base(message) { }
}
