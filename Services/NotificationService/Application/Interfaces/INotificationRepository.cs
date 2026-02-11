using NotificationService.Domain.Entities;

namespace NotificationService.Application.Interfaces;

public interface INotificationRepository
{
    Task AddAsync(Notification notification, CancellationToken ct);
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Notification?> GetByIdForUpdateAsync(Guid id, CancellationToken ct);
    Task<List<Notification>> GetForUserAsync(
        Guid userId,
        bool? isRead,
        DateTime? from,
        DateTime? to,
        int take,
        CancellationToken ct);
}
