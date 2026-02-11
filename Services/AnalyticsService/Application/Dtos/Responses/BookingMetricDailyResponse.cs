namespace AnalyticsService.Application.Dtos.Responses;

public record BookingMetricDailyResponse(
    Guid Id,
    Guid PropertyId,
    Guid? UnitId,
    DateOnly Date,
    int TotalBookings,
    int ConfirmedBookings,
    int CancelledBookings
);
