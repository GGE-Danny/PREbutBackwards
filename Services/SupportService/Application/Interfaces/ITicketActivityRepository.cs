using SupportService.Domain.Entities;

namespace SupportService.Application.Interfaces;

public interface ITicketActivityRepository
{
    Task<List<TicketActivity>> GetByTicketAsync(Guid ticketId, CancellationToken ct);
    Task AddAsync(TicketActivity activity, CancellationToken ct);
}
