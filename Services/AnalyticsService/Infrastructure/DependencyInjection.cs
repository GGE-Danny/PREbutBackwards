using MassTransit;
using Microsoft.EntityFrameworkCore;
using AnalyticsService.Application.Interfaces;
using AnalyticsService.Infrastructure.Consumers;
using AnalyticsService.Infrastructure.Persistence;
using AnalyticsService.Infrastructure.Repositories;

namespace AnalyticsService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<AnalyticsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IProcessedEventRepository, ProcessedEventRepository>();
        services.AddScoped<IBookingMetricRepository, BookingMetricRepository>();
        services.AddScoped<IVacancyMetricRepository, VacancyMetricRepository>();
        services.AddScoped<IRevenueMetricRepository, RevenueMetricRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // MassTransit + RabbitMQ
        services.AddMassTransit(x =>
        {
            x.AddConsumer<BookingConfirmedConsumer>();
            x.AddConsumer<BookingCancelledConsumer>();
            x.AddConsumer<PaymentRecordedConsumer>();
            x.AddConsumer<ExpenseLoggedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitConfig = configuration.GetSection("RabbitMQ");
                cfg.Host(rabbitConfig["Host"], rabbitConfig["VirtualHost"], h =>
                {
                    h.Username(rabbitConfig["Username"]!);
                    h.Password(rabbitConfig["Password"]!);
                });

                cfg.ReceiveEndpoint("analytics-booking-confirmed", e =>
                {
                    e.ConfigureConsumer<BookingConfirmedConsumer>(context);
                });

                cfg.ReceiveEndpoint("analytics-booking-cancelled", e =>
                {
                    e.ConfigureConsumer<BookingCancelledConsumer>(context);
                });

                cfg.ReceiveEndpoint("analytics-payment-recorded", e =>
                {
                    e.ConfigureConsumer<PaymentRecordedConsumer>(context);
                });

                cfg.ReceiveEndpoint("analytics-expense-logged", e =>
                {
                    e.ConfigureConsumer<ExpenseLoggedConsumer>(context);
                });
            });
        });

        return services;
    }
}
