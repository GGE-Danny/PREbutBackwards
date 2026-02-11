using MassTransit;
using AnalyticsService.Application.Interfaces;
using AnalyticsService.Domain.Entities;
using AnalyticsService.Infrastructure.Consumers.Contracts;

namespace AnalyticsService.Infrastructure.Consumers;

public sealed class BookingCancelledConsumer : IConsumer<BookingCancelledEvent>
{
    private readonly IProcessedEventRepository _processedEvents;
    private readonly IBookingMetricRepository _bookingMetrics;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<BookingCancelledConsumer> _logger;

    public BookingCancelledConsumer(
        IProcessedEventRepository processedEvents,
        IBookingMetricRepository bookingMetrics,
        IUnitOfWork uow,
        ILogger<BookingCancelledConsumer> logger)
    {
        _processedEvents = processedEvents;
        _bookingMetrics = bookingMetrics;
        _uow = uow;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BookingCancelledEvent> context)
    {
        var evt = context.Message;
        var messageId = context.MessageId?.ToString() ?? $"cancel-{evt.BookingId}";

        _logger.LogInformation(
            "Received BookingCancelledEvent: BookingId={BookingId}, PropertyId={PropertyId}",
            evt.BookingId, evt.PropertyId);

        // Idempotency check
        if (await _processedEvents.ExistsAsync(messageId, context.CancellationToken))
        {
            _logger.LogInformation("Event {MessageId} already processed, skipping", messageId);
            return;
        }

        await _uow.BeginTransactionAsync(context.CancellationToken);

        try
        {
            var metricDate = DateOnly.FromDateTime(evt.CancelledAt);

            var bookingMetric = await _bookingMetrics.GetByKeyForUpdateAsync(
                evt.PropertyId, evt.UnitId, metricDate, context.CancellationToken);

            if (bookingMetric is null)
            {
                bookingMetric = new BookingMetricDaily
                {
                    PropertyId = evt.PropertyId,
                    UnitId = evt.UnitId,
                    Date = metricDate,
                    TotalBookings = 1,
                    ConfirmedBookings = 0,
                    CancelledBookings = 1
                };
                await _bookingMetrics.AddAsync(bookingMetric, context.CancellationToken);
            }
            else
            {
                bookingMetric.TotalBookings++;
                bookingMetric.CancelledBookings++;
                bookingMetric.UpdatedAt = DateTime.UtcNow;
            }

            await _processedEvents.AddAsync(new ProcessedEvent
            {
                MessageId = messageId,
                ProcessedAt = DateTime.UtcNow
            }, context.CancellationToken);

            await _uow.SaveChangesAsync(context.CancellationToken);
            await _uow.CommitAsync(context.CancellationToken);

            _logger.LogInformation("Processed BookingCancelledEvent {BookingId}", evt.BookingId);
        }
        catch
        {
            await _uow.RollbackAsync(context.CancellationToken);
            throw;
        }
    }
}
