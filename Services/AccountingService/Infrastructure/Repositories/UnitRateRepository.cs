using AccountingService.Application.Interfaces;
using AccountingService.Domain.Entities;
using AccountingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccountingService.Infrastructure.Repositories;

public sealed class UnitRateRepository : IUnitRateRepository
{
    private readonly AccountingDbContext _db;
    public UnitRateRepository(AccountingDbContext db) => _db = db;

    public async Task<UnitRate?> GetActiveRateForUnitAsync(Guid unitId, DateOnly? asOfDate, CancellationToken ct)
    {
        var query = _db.UnitRates
            .AsNoTracking()
            .Where(r => r.UnitId == unitId && r.IsActive);

        if (asOfDate.HasValue)
        {
            var date = asOfDate.Value;
            query = query.Where(r =>
                (r.EffectiveFrom == null || r.EffectiveFrom <= date) &&
                (r.EffectiveTo == null || r.EffectiveTo >= date));
        }

        return await query.FirstOrDefaultAsync(ct);
    }

    public Task<UnitRate?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.UnitRates.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<UnitRate?> GetByIdForUpdateAsync(Guid id, CancellationToken ct)
        => _db.UnitRates.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<List<UnitRate>> GetByUnitAsync(Guid unitId, CancellationToken ct)
        => await _db.UnitRates
            .AsNoTracking()
            .Where(r => r.UnitId == unitId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

    public async Task DeactivateActiveRateAsync(Guid unitId, DateOnly effectiveTo, CancellationToken ct)
    {
        var activeRate = await _db.UnitRates
            .FirstOrDefaultAsync(r => r.UnitId == unitId && r.IsActive, ct);

        if (activeRate is not null)
        {
            activeRate.IsActive = false;
            activeRate.EffectiveTo = effectiveTo;
            activeRate.UpdatedAt = DateTime.UtcNow;
        }
    }

    public async Task AddAsync(UnitRate unitRate, CancellationToken ct)
    {
        await _db.UnitRates.AddAsync(unitRate, ct);
    }
}
