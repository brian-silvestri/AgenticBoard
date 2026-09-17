namespace AgenticBoard.Application.Features.Auth.Dtos;

public record UserDto(
    int Id,
    string Email,
    string FullName,
    DateTime CreatedAt
);
