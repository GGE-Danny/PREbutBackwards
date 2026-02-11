using MassTransit;
using AnalyticsService.Application.Interfaces;
using AnalyticsService.Domain.Entities;
using AnalyticsService.Infrastructure.Consumers.Contracts;

namespace AnalyticsService.Infrastructure.Consumers;

public sealed class BookingConfirmedConsumer : IConsumer<BookingConfirmedEvent>
{
    private readonly IProcessedEventRepository _processedEvents;
    private readonly IBookingMetricRepository _bookingMetrics;
    private readonly IVacancyMetricRepository _vacancyMetrics;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<BookingConfirmedConsumer> _logger;

    public BookingConfirmedConsumer(
        IProcessedEventRepository processedEvents,
        IBookingMetricRepository bookingMetrics,
        IVacancyMetricRepository vacancyMetrics,
        IUnitOfWork uow,
        ILogger<BookingConfirmedConsumer> logger)
    {
        _processedEvents = processedEvents;
        _bookingMetrics = bookingMetrics;
        _vacancyMetrics = vacancyMetrics;
        _uow = uow;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BookingConfirmedEvent> context)
    {
        var evt = context.Message;
        var messageId = context.MessageId?.ToString() ?? evt.BookingId.ToString();

        _logger.LogInformation(
            "Received BookingConfirmedEvent: BookingId={BookingId}, PropertyId={PropertyId}",
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
            // 1. Update daily booking metrics
            var metricDate = evt.ConfirmedAt.HasValue
                ? DateOnly.FromDateTime(evt.ConfirmedAt.Value)
                : DateOnly.FromDateTime(DateTime.UtcNow);

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
                    ConfirmedBookings = 1,
                    CancelledBookings = 0
                };
                await _bookingMetrics.AddAsync(bookingMetric, context.CancellationToken);
            }
            else
            {
                bookingMetric.TotalBookings++;
                bookingMetric.ConfirmedBookings++;
                bookingMetric.UpdatedAt = DateTime.UtcNow;
            }

            // 2. Update vacancy metrics (expand date range day-by-day)
            for (var date = evt.StartDate; date <= evt.EndDate; date = date.AddDays(1))
            {
                var vacancyMetric = await _vacancyMetrics.GetByKeyForUpdateAsync(
                    evt.PropertyId, evt.UnitId, date.Year, date.Month, context.CancellationToken);

                if (vacancyMetric is null)
                {
                    vacancyMetric = new VacancyMetricMonthly
                    {
                        PropertyId = evt.PropertyId,
                        UnitId = evt.UnitId,
                        Year = date.Year,
                        Month = date.Month,
                        OccupiedDays = 1,
                        AvailableDays = 30 // MVP: fixed
                    };
                    await _vacancyMetrics.AddAsync(vacancyMetric, context.CancellationToken);
                }
                else
                {
                    vacancyMetric.OccupiedDays++;
                    vacancyMetric.UpdatedAt = DateTime.UtcNow;
                }
            }

            // 3. Record processed event
            await _processedEvents.AddAsync(new ProcessedEvent
            {
                MessageId = messageId,
                ProcessedAt = DateTime.UtcNow
            }, context.CancellationToken);

            await _uow.SaveChangesAsync(context.CancellationToken);
            await _uow.CommitAsync(context.CancellationToken);

            _logger.LogInformation("Processed BookingConfirmedEvent {BookingId}", evt.BookingId);
        }
        catch
        {
            await _uow.RollbackAsync(context.CancellationToken);
            throw;
        }
    }
}
