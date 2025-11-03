using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Infrastructure.Config
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService.Application.Services.AuthService>();
            services.AddScoped<ITokenService, TokenService>();
            return services;
        }
    }
}
