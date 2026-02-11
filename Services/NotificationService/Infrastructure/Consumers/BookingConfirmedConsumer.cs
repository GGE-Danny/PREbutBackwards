using System.Text.Json;
using MassTransit;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Infrastructure.Consumers.Contracts;

namespace NotificationService.Infrastructure.Consumers;

public sealed class BookingConfirmedConsumer : IConsumer<BookingConfirmedEvent>
{
    private readonly INotificationRepository _notifications;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<BookingConfirmedConsumer> _logger;

    public BookingConfirmedConsumer(
        INotificationRepository notifications,
        IUnitOfWork uow,
        ILogger<BookingConfirmedConsumer> logger)
    {
        _notifications = notifications;
        _uow = uow;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BookingConfirmedEvent> context)
    {
        var evt = context.Message;

        _logger.LogInformation(
            "Received BookingConfirmedEvent: BookingId={BookingId}, TenantUserId={TenantUserId}",
            evt.BookingId, evt.TenantUserId);

        var metadata = JsonSerializer.Serialize(new
        {
            evt.BookingId,
            evt.PropertyId,
            evt.UnitId,
            StartDate = evt.StartDate.ToString("yyyy-MM-dd"),
            EndDate = evt.EndDate.ToString("yyyy-MM-dd")
        });

        var notification = new Notification
        {
            RecipientUserId = evt.TenantUserId,
            RecipientType = RecipientType.User,
            Type = NotificationType.BookingConfirmed,
            Channel = NotificationChannel.InApp,
            Title = "Booking Confirmed",
            Message = $"Your booking has been confirmed for {evt.StartDate:MMM dd, yyyy} to {evt.EndDate:MMM dd, yyyy}.",
            MetadataJson = metadata,
            Status = NotificationStatus.Pending,
            IsRead = false
        };

        await _notifications.AddAsync(notification, context.CancellationToken);
        await _uow.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Created notification {NotificationId} for TenantUserId={TenantUserId}",
            notification.Id, evt.TenantUserId);
    }
}
