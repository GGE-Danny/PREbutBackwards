using AccountingService.Application.Interfaces;
using AccountingService.Domain.Entities;
using AccountingService.Domain.Enums;
using AccountingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccountingService.Infrastructure.Repositories;

public sealed class InvoiceRepository : IInvoiceRepository
{
    private readonly AccountingDbContext _db;
    public InvoiceRepository(AccountingDbContext db) => _db = db;

    public Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Invoices.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Invoice?> GetByIdForUpdateAsync(Guid id, CancellationToken ct)
        => _db.Invoices.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<decimal> SumRentCollectedAsync(Guid propertyId, DateOnly from, DateOnly to, CancellationToken ct)
    {
        // Must use UTC for PostgreSQL timestamptz columns
        var fromUtc = DateTime.SpecifyKind(from.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var toUtc = DateTime.SpecifyKind(to.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);

        // Sum payments for Rent invoices for this property, where payment was made in period
        return await _db.Payments
            .AsNoTracking()
            .Where(p => p.CreatedAt >= fromUtc
                     && p.CreatedAt <= toUtc
                     && p.Invoice != null
                     && p.Invoice.PropertyId == propertyId
                     && p.Invoice.Type == InvoiceType.Rent)
            .SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;
    }
}
