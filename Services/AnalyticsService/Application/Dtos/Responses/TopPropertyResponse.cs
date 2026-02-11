namespace AnalyticsService.Application.Dtos.Responses;

public record TopPropertyResponse(
    Guid PropertyId,
    int TotalBookings,
    int Rank
);
