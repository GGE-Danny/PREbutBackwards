using Microsoft.EntityFrameworkCore;
using AnalyticsService.Application.Interfaces;
using AnalyticsService.Domain.Entities;
using AnalyticsService.Infrastructure.Persistence;

namespace AnalyticsService.Infrastructure.Repositories;

public sealed class ProcessedEventRepository : IProcessedEventRepository
{
    private readonly AnalyticsDbContext _db;
    public ProcessedEventRepository(AnalyticsDbContext db) => _db = db;

    public Task<bool> ExistsAsync(string messageId, CancellationToken ct)
        => _db.ProcessedEvents.AnyAsync(x => x.MessageId == messageId, ct);

    public async Task AddAsync(ProcessedEvent evt, CancellationToken ct)
        => await _db.ProcessedEvents.AddAsync(evt, ct);
}
