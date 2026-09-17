using System.Reflection;
using AgenticBoard.Application.Features.Auth.Services;
using AgenticBoard.Application.Features.Projects.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AgenticBoard.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProjectService, ProjectService>();

        return services;
    }
}
