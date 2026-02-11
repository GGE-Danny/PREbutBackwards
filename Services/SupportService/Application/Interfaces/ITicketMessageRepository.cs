using SupportService.Domain.Entities;

namespace SupportService.Application.Interfaces;

public interface ITicketMessageRepository
{
    Task<List<TicketMessage>> GetByTicketAsync(Guid ticketId, CancellationToken ct);
    Task AddAsync(TicketMessage message, CancellationToken ct);
}
