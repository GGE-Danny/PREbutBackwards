using SupportService.Domain.Entities;

namespace SupportService.Application.Interfaces;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Ticket?> GetByIdForUpdateAsync(Guid id, CancellationToken ct);
    Task<List<Ticket>> GetByTenantAsync(Guid tenantUserId, CancellationToken ct);
    Task AddAsync(Ticket ticket, CancellationToken ct);
}
