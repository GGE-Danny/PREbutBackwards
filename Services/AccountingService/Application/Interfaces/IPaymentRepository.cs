using AccountingService.Domain.Entities;

namespace AccountingService.Application.Interfaces;

public interface IPaymentRepository
{
    Task<bool> ExistsByReferenceIdAsync(string referenceId, CancellationToken ct);
    Task AddAsync(Payment payment, CancellationToken ct);
}
