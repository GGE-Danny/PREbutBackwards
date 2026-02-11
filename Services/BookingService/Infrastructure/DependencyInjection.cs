using BookingService.Infrastructure.Clients;
using BookingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBookingInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var conn = config.GetConnectionString("BookingDb");
        services.AddDbContext<BookingDbContext>(opt => opt.UseNpgsql(conn));

        services.AddHttpContextAccessor();

        services.AddHttpClient<IPropertyServiceClient, PropertyServiceClient>(client =>
        {
            client.BaseAddress = new Uri(config["Services:PropertyService:BaseUrl"]!);
        });

        return services;
    }
}
