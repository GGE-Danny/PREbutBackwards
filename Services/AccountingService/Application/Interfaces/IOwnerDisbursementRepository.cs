using AccountingService.Domain.Entities;

namespace AccountingService.Application.Interfaces;

public interface IOwnerDisbursementRepository
{
    Task AddAsync(OwnerDisbursement disbursement, CancellationToken ct);
    Task<OwnerDisbursement?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<OwnerDisbursement?> GetByIdForUpdateAsync(Guid id, CancellationToken ct);
    Task<OwnerDisbursement?> FindByOwnerPropertyPeriodAsync(Guid ownerId, Guid propertyId, DateOnly periodStart, DateOnly periodEnd, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
