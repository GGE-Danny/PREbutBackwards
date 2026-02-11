using AccountingService.Application.Interfaces;
using AccountingService.Domain.Entities;
using AccountingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccountingService.Infrastructure.Repositories;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly AccountingDbContext _db;
    public PaymentRepository(AccountingDbContext db) => _db = db;

    public Task<bool> ExistsByReferenceIdAsync(string referenceId, CancellationToken ct)
        => _db.Payments.AsNoTracking().AnyAsync(x => x.ReferenceId == referenceId, ct);

    public async Task AddAsync(Payment payment, CancellationToken ct)
        => await _db.Payments.AddAsync(payment, ct);
}
