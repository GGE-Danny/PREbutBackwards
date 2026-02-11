using Microsoft.EntityFrameworkCore;
using SalesService.Application.Interfaces;
using SalesService.Domain.Entities;
using SalesService.Infrastructure.Persistence;

namespace SalesService.Infrastructure.Repositories;

public sealed class VisitRepository : IVisitRepository
{
    private readonly SalesDbContext _db;
    public VisitRepository(SalesDbContext db) => _db = db;

    public Task<Visit?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Visits.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Visit?> GetByIdForUpdateAsync(Guid id, CancellationToken ct)
        => _db.Visits.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<List<Visit>> GetByLeadAsync(Guid leadId, CancellationToken ct)
        => await _db.Visits
            .AsNoTracking()
            .Where(x => x.LeadId == leadId)
            .OrderByDescending(x => x.ScheduledAt)
            .ToListAsync(ct);

    public async Task AddAsync(Visit visit, CancellationToken ct)
        => await _db.Visits.AddAsync(visit, ct);
}
