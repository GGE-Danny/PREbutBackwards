using Microsoft.EntityFrameworkCore;
using SupportService.Application.Interfaces;
using SupportService.Domain.Entities;
using SupportService.Infrastructure.Persistence;

namespace SupportService.Infrastructure.Repositories;

public sealed class TicketMessageRepository : ITicketMessageRepository
{
    private readonly SupportDbContext _db;
    public TicketMessageRepository(SupportDbContext db) => _db = db;

    public async Task<List<TicketMessage>> GetByTicketAsync(Guid ticketId, CancellationToken ct)
        => await _db.TicketMessages
            .AsNoTracking()
            .Where(x => x.TicketId == ticketId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(TicketMessage message, CancellationToken ct)
        => await _db.TicketMessages.AddAsync(message, ct);
}
