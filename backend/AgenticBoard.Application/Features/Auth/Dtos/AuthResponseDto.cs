namespace AgenticBoard.Application.Features.Auth.Dtos;

public record AuthResponseDto(
    string Token,
    UserDto User
);
