using AccountingService.Domain.Entities;

namespace AccountingService.Application.Interfaces;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Invoice?> GetByIdForUpdateAsync(Guid id, CancellationToken ct);
    Task<decimal> SumRentCollectedAsync(Guid propertyId, DateOnly from, DateOnly to, CancellationToken ct);
}
