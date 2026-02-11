using System.Text.Json;
using MassTransit;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Infrastructure.Consumers.Contracts;

namespace NotificationService.Infrastructure.Consumers;

public sealed class TicketCreatedConsumer : IConsumer<TicketCreatedEvent>
{
    private readonly INotificationRepository _notifications;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<TicketCreatedConsumer> _logger;

    public TicketCreatedConsumer(
        INotificationRepository notifications,
        IUnitOfWork uow,
        ILogger<TicketCreatedConsumer> logger)
    {
        _notifications = notifications;
        _uow = uow;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TicketCreatedEvent> context)
    {
        var evt = context.Message;

        _logger.LogInformation(
            "Received TicketCreatedEvent: TicketId={TicketId}, CreatedByUserId={CreatedByUserId}",
            evt.TicketId, evt.CreatedByUserId);

        var metadata = JsonSerializer.Serialize(new
        {
            evt.TicketId,
            evt.Subject,
            evt.Status
        });

        // Notify the creator of the ticket
        var notification = new Notification
        {
            RecipientUserId = evt.CreatedByUserId,
            RecipientType = RecipientType.User,
            Type = NotificationType.TicketCreated,
            Channel = NotificationChannel.InApp,
            Title = "Support Ticket Created",
            Message = $"Your support ticket \"{evt.Subject}\" has been created and is being reviewed.",
            MetadataJson = metadata,
            Status = NotificationStatus.Pending,
            IsRead = false
        };

        await _notifications.AddAsync(notification, context.CancellationToken);
        await _uow.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Created notification {NotificationId} for TicketId={TicketId}",
            notification.Id, evt.TicketId);
    }
}
