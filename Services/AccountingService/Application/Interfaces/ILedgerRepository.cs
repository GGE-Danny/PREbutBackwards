using AccountingService.Domain.Entities;

namespace AccountingService.Application.Interfaces;

public interface ILedgerRepository
{
    Task AddAsync(LedgerEntry entry, CancellationToken ct);
}
