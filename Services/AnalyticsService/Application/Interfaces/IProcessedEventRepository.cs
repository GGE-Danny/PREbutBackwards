using AnalyticsService.Domain.Entities;

namespace AnalyticsService.Application.Interfaces;

public interface IProcessedEventRepository
{
    Task<bool> ExistsAsync(string messageId, CancellationToken ct);
    Task AddAsync(ProcessedEvent evt, CancellationToken ct);
}
