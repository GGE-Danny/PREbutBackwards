using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Infrastructure.Repositories;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _db;
    public NotificationRepository(NotificationDbContext db) => _db = db;

    public async Task AddAsync(Notification notification, CancellationToken ct)
        => await _db.Notifications.AddAsync(notification, ct);

    public Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Notifications.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Notification?> GetByIdForUpdateAsync(Guid id, CancellationToken ct)
        => _db.Notifications.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<List<Notification>> GetForUserAsync(
        Guid userId,
        bool? isRead,
        DateTime? from,
        DateTime? to,
        int take,
        CancellationToken ct)
    {
        var query = _db.Notifications
            .AsNoTracking()
            .Where(x => x.RecipientUserId == userId);

        if (isRead.HasValue)
            query = query.Where(x => x.IsRead == isRead.Value);

        if (from.HasValue)
        {
            var fromUtc = DateTime.SpecifyKind(from.Value, DateTimeKind.Utc);
            query = query.Where(x => x.CreatedAt >= fromUtc);
        }

        if (to.HasValue)
        {
            var toUtc = DateTime.SpecifyKind(to.Value, DateTimeKind.Utc);
            query = query.Where(x => x.CreatedAt <= toUtc);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(ct);
    }
}
