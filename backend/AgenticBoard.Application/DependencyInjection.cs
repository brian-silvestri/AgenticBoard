using System.Reflection;
using AgenticBoard.Application.Features.Auth.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AgenticBoard.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
