namespace AgenticBoard.Application.Features.Auth.Dtos;

public record RegisterDto(
    string Email,
    string FullName,
    string Password
);
