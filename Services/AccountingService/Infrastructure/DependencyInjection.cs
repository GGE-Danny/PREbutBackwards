using AccountingService.Application.Interfaces;
using AccountingService.Infrastructure.Consumers;
using AccountingService.Infrastructure.Persistence;
using AccountingService.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AccountingService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAccountingInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var conn = config.GetConnectionString("AccountingDb");
        services.AddDbContext<AccountingDbContext>(opt => opt.UseNpgsql(conn));

        // Repos
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IOwnerDisbursementRepository, OwnerDisbursementRepository>();
        services.AddScoped<ILedgerRepository, LedgerRepository>();
        services.AddScoped<IUnitRateRepository, UnitRateRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Commission logic
        services.AddSingleton<ICommissionCalculator, CommissionCalculator>();

        // MassTransit
        services.AddMassTransit(x =>
        {
            x.AddConsumer<BookingConfirmedConsumer>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(config["RabbitMq:Host"], h =>
                {
                    h.Username(config["RabbitMq:Username"]);
                    h.Password(config["RabbitMq:Password"]);
                });

                cfg.ReceiveEndpoint("booking-confirmed-accounting", e =>
                {
                    e.ConfigureConsumer<BookingConfirmedConsumer>(ctx);
                });
            });
        });

        return services;
    }
}
