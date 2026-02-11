using Microsoft.EntityFrameworkCore;
using SalesService.Application.Interfaces;
using SalesService.Domain.Entities;
using SalesService.Infrastructure.Persistence;

namespace SalesService.Infrastructure.Repositories;

public sealed class CommissionRepository : ICommissionRepository
{
    private readonly SalesDbContext _db;
    public CommissionRepository(SalesDbContext db) => _db = db;

    public Task<CommissionRecord?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.CommissionRecords.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<CommissionRecord?> GetByIdForUpdateAsync(Guid id, CancellationToken ct)
        => _db.CommissionRecords.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<CommissionRecord?> GetByBookingIdAsync(Guid bookingId, CancellationToken ct)
        => _db.CommissionRecords.AsNoTracking().FirstOrDefaultAsync(x => x.BookingId == bookingId, ct);

    public async Task<List<CommissionRecord>> QueryAsync(
        Guid? propertyId, Guid? ownerId, DateOnly? from, DateOnly? to, CancellationToken ct)
    {
        var query = _db.CommissionRecords.AsNoTracking().AsQueryable();

        if (propertyId.HasValue)
            query = query.Where(x => x.PropertyId == propertyId.Value);

        if (ownerId.HasValue)
            query = query.Where(x => x.OwnerId == ownerId.Value);

        if (from.HasValue)
        {
            var fromUtc = DateTime.SpecifyKind(from.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            query = query.Where(x => x.EarnedAt >= fromUtc);
        }

        if (to.HasValue)
        {
            var toUtc = DateTime.SpecifyKind(to.Value.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);
            query = query.Where(x => x.EarnedAt <= toUtc);
        }

        return await query.OrderByDescending(x => x.EarnedAt).ToListAsync(ct);
    }

    public async Task AddAsync(CommissionRecord record, CancellationToken ct)
        => await _db.CommissionRecords.AddAsync(record, ct);
}
