using Microsoft.EntityFrameworkCore;
using SupportService.Application.Interfaces;
using SupportService.Domain.Entities;
using SupportService.Infrastructure.Persistence;

namespace SupportService.Infrastructure.Repositories;

public sealed class TicketActivityRepository : ITicketActivityRepository
{
    private readonly SupportDbContext _db;
    public TicketActivityRepository(SupportDbContext db) => _db = db;

    public async Task<List<TicketActivity>> GetByTicketAsync(Guid ticketId, CancellationToken ct)
        => await _db.TicketActivities
            .AsNoTracking()
            .Where(x => x.TicketId == ticketId)
            .OrderBy(x => x.OccurredAt)
            .ToListAsync(ct);

    public async Task AddAsync(TicketActivity activity, CancellationToken ct)
        => await _db.TicketActivities.AddAsync(activity, ct);
}
