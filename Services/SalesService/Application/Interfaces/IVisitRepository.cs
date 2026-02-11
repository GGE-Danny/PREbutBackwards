using SalesService.Domain.Entities;

namespace SalesService.Application.Interfaces;

public interface IVisitRepository
{
    Task<Visit?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Visit?> GetByIdForUpdateAsync(Guid id, CancellationToken ct);
    Task<List<Visit>> GetByLeadAsync(Guid leadId, CancellationToken ct);
    Task AddAsync(Visit visit, CancellationToken ct);
}
