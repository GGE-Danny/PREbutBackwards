using SalesService.Domain.Entities;

namespace SalesService.Application.Interfaces;

public interface ICommissionRepository
{
    Task<CommissionRecord?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<CommissionRecord?> GetByIdForUpdateAsync(Guid id, CancellationToken ct);
    Task<CommissionRecord?> GetByBookingIdAsync(Guid bookingId, CancellationToken ct);
    Task<List<CommissionRecord>> QueryAsync(Guid? propertyId, Guid? ownerId, DateOnly? from, DateOnly? to, CancellationToken ct);
    Task AddAsync(CommissionRecord record, CancellationToken ct);
}
