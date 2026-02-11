namespace BookingService.Infrastructure.Clients;

public interface IPropertyServiceClient
{
    Task<bool> UnitBelongsToPropertyAsync(Guid propertyId, Guid unitId, CancellationToken ct = default);
}
