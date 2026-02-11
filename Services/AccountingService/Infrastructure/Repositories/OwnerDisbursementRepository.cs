using AccountingService.Application.Interfaces;
using AccountingService.Domain.Entities;
using AccountingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccountingService.Infrastructure.Repositories;

public sealed class OwnerDisbursementRepository : IOwnerDisbursementRepository
{
    private readonly AccountingDbContext _db;
    public OwnerDisbursementRepository(AccountingDbContext db) => _db = db;

    public async Task AddAsync(OwnerDisbursement disbursement, CancellationToken ct)
        => await _db.OwnerDisbursements.AddAsync(disbursement, ct);

    public Task<OwnerDisbursement?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.OwnerDisbursements.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<OwnerDisbursement?> GetByIdForUpdateAsync(Guid id, CancellationToken ct)
        => _db.OwnerDisbursements.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<OwnerDisbursement?> FindByOwnerPropertyPeriodAsync(Guid ownerId, Guid propertyId, DateOnly periodStart, DateOnly periodEnd, CancellationToken ct)
        => _db.OwnerDisbursements.AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.OwnerId == ownerId &&
                x.PropertyId == propertyId &&
                x.PeriodStart == periodStart &&
                x.PeriodEnd == periodEnd, ct);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
