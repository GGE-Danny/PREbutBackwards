using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupportService.Application.Interfaces;
using SupportService.Infrastructure.Persistence;
using SupportService.Infrastructure.Repositories;

namespace SupportService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core + PostgreSQL
        services.AddDbContext<SupportDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("SupportDb")));

        // Repositories
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<ITicketMessageRepository, TicketMessageRepository>();
        services.AddScoped<ITicketActivityRepository, TicketActivityRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
