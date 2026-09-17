using AgenticBoard.Application.Features.Auth.Dtos;

namespace AgenticBoard.Application.Features.Auth.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
    Task<UserDto> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
