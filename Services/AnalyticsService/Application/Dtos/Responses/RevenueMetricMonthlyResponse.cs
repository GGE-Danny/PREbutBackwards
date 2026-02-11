namespace AnalyticsService.Application.Dtos.Responses;

public record RevenueMetricMonthlyResponse(
    Guid Id,
    Guid PropertyId,
    int Year,
    int Month,
    decimal RentCollected,
    decimal Expenses,
    decimal NetRevenue
);
