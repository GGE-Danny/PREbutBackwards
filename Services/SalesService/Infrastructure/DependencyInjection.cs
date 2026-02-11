using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SalesService.Application.Interfaces;
using SalesService.Infrastructure.Consumers;
using SalesService.Infrastructure.Persistence;
using SalesService.Infrastructure.Repositories;

namespace SalesService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core + PostgreSQL
        services.AddDbContext<SalesDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("SalesDb")));

        // Repositories
        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddScoped<IVisitRepository, VisitRepository>();
        services.AddScoped<ICommissionRepository, CommissionRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // MassTransit + RabbitMQ
        services.AddMassTransit(x =>
        {
            x.AddConsumer<BookingConfirmedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitConfig = configuration.GetSection("RabbitMq");
                cfg.Host(rabbitConfig["Host"], h =>
                {
                    h.Username(rabbitConfig["Username"]);
                    h.Password(rabbitConfig["Password"]);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
