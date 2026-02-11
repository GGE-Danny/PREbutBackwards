namespace TenantService.Infrastructure.Clients
{
    public interface IPropertyServiceClient
    {
        Task<bool> UnitBelongsToPropertyAsync(Guid propertyId, Guid unitId);
    }

}
