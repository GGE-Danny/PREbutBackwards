namespace PropertyService.Infrastructure.Clients;

public interface ITenantServiceClient
{
    Task NotifyOccupancyAssignedAsync(
        Guid tenantUserId,
        Guid propertyId,
        Guid unitId,
        DateOnly startDate);

    Task NotifyOccupancyVacatedAsync(
        Guid tenantUserId,
        Guid propertyId,
        Guid unitId,
        DateOnly endDate);
}
