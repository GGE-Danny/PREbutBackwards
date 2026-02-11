using Microsoft.EntityFrameworkCore;
using SalesService.Application.Interfaces;
using SalesService.Domain.Entities;
using SalesService.Domain.Enums;
using SalesService.Infrastructure.Persistence;

namespace SalesService.Infrastructure.Repositories;

public sealed class LeadRepository : ILeadRepository
{
    private readonly SalesDbContext _db;
    public LeadRepository(SalesDbContext db) => _db = db;

    public Task<Lead?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Leads.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Lead?> GetByIdForUpdateAsync(Guid id, CancellationToken ct)
        => _db.Leads.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<List<Lead>> GetByPropertyAsync(Guid propertyId, CancellationToken ct)
        => await _db.Leads
            .AsNoTracking()
            .Where(x => x.PropertyId == propertyId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

    public Task<Lead?> FindByTenantPropertyUnitAsync(Guid tenantUserId, Guid propertyId, Guid? unitId, CancellationToken ct)
        => _db.Leads
            .Where(x => x.TenantUserId == tenantUserId
                     && x.PropertyId == propertyId
                     && x.UnitId == unitId
                     && x.Status != LeadStatus.Converted
                     && x.Status != LeadStatus.Lost)
            .FirstOrDefaultAsync(ct);

    public async Task AddAsync(Lead lead, CancellationToken ct)
        => await _db.Leads.AddAsync(lead, ct);
}
