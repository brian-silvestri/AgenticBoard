using System.Security.Claims;
using AgenticBoard.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AgenticBoard.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId
    {
        get
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            var subClaim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                           ?? principal?.FindFirst("sub")?.Value;

            if (int.TryParse(subClaim, out var id))
            {
                return id;
            }

            return null;
        }
    }

    public string? Email
    {
        get
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            return principal?.FindFirst(ClaimTypes.Email)?.Value 
                   ?? principal?.FindFirst("email")?.Value;
        }
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
