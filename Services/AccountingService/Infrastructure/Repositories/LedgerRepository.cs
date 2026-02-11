using AccountingService.Application.Interfaces;
using AccountingService.Domain.Entities;
using AccountingService.Infrastructure.Persistence;

namespace AccountingService.Infrastructure.Repositories;

public sealed class LedgerRepository : ILedgerRepository
{
    private readonly AccountingDbContext _db;
    public LedgerRepository(AccountingDbContext db) => _db = db;

    public async Task AddAsync(LedgerEntry entry, CancellationToken ct)
        => await _db.LedgerEntries.AddAsync(entry, ct);
}
