namespace AnalyticsService.Application.Dtos.Responses;

public record VacancyMetricMonthlyResponse(
    Guid Id,
    Guid PropertyId,
    Guid UnitId,
    int Year,
    int Month,
    int OccupiedDays,
    int AvailableDays,
    decimal OccupancyRate // OccupiedDays / AvailableDays * 100
);
