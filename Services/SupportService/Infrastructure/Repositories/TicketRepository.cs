using Microsoft.EntityFrameworkCore;
using SupportService.Application.Interfaces;
using SupportService.Domain.Entities;
using SupportService.Infrastructure.Persistence;

namespace SupportService.Infrastructure.Repositories;

public sealed class TicketRepository : ITicketRepository
{
    private readonly SupportDbContext _db;
    public TicketRepository(SupportDbContext db) => _db = db;

    public Task<Ticket?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Tickets.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Ticket?> GetByIdForUpdateAsync(Guid id, CancellationToken ct)
        => _db.Tickets.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<List<Ticket>> GetByTenantAsync(Guid tenantUserId, CancellationToken ct)
        => await _db.Tickets
            .AsNoTracking()
            .Where(x => x.TenantUserId == tenantUserId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(Ticket ticket, CancellationToken ct)
        => await _db.Tickets.AddAsync(ticket, ct);
}
