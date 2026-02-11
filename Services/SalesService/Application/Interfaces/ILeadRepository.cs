using SalesService.Domain.Entities;

namespace SalesService.Application.Interfaces;

public interface ILeadRepository
{
    Task<Lead?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Lead?> GetByIdForUpdateAsync(Guid id, CancellationToken ct);
    Task<List<Lead>> GetByPropertyAsync(Guid propertyId, CancellationToken ct);
    Task<Lead?> FindByTenantPropertyUnitAsync(Guid tenantUserId, Guid propertyId, Guid? unitId, CancellationToken ct);
    Task AddAsync(Lead lead, CancellationToken ct);
}
