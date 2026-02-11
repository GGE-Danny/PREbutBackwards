using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AnalyticsService.Application.Dtos.Responses;
using AnalyticsService.Application.Interfaces;

namespace AnalyticsService.Api.Controllers;

[Authorize(Policy = "RequireAuthenticatedUser")]
public class AnalyticsController : ApiControllerBase
{
    private readonly IBookingMetricRepository _bookingMetrics;
    private readonly IVacancyMetricRepository _vacancyMetrics;
    private readonly IRevenueMetricRepository _revenueMetrics;

    public AnalyticsController(
        IBookingMetricRepository bookingMetrics,
        IVacancyMetricRepository vacancyMetrics,
        IRevenueMetricRepository revenueMetrics)
    {
        _bookingMetrics = bookingMetrics;
        _vacancyMetrics = vacancyMetrics;
        _revenueMetrics = revenueMetrics;
    }

    /// <summary>
    /// Get daily booking metrics for a property within a date range.
    /// </summary>
    [HttpGet("bookings/daily")]
    public async Task<ActionResult<List<BookingMetricDailyResponse>>> GetBookingsDaily(
        [FromQuery] Guid propertyId,
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken ct)
    {
        var metrics = await _bookingMetrics.GetDailyMetricsAsync(propertyId, from, to, ct);

        var response = metrics.Select(m => new BookingMetricDailyResponse(
            m.Id,
            m.PropertyId,
            m.UnitId,
            m.Date,
            m.TotalBookings,
            m.ConfirmedBookings,
            m.CancelledBookings
        )).ToList();

        return Ok(response);
    }

    /// <summary>
    /// Get monthly vacancy metrics for a property.
    /// </summary>
    [HttpGet("vacancy/monthly")]
    public async Task<ActionResult<List<VacancyMetricMonthlyResponse>>> GetVacancyMonthly(
        [FromQuery] Guid propertyId,
        [FromQuery] int year,
        CancellationToken ct)
    {
        var metrics = await _vacancyMetrics.GetMonthlyMetricsAsync(propertyId, year, ct);

        var response = metrics.Select(m => new VacancyMetricMonthlyResponse(
            m.Id,
            m.PropertyId,
            m.UnitId,
            m.Year,
            m.Month,
            m.OccupiedDays,
            m.AvailableDays,
            m.AvailableDays > 0
                ? Math.Round((decimal)m.OccupiedDays / m.AvailableDays * 100, 2)
                : 0m
        )).ToList();

        return Ok(response);
    }

    /// <summary>
    /// Get monthly revenue metrics for a property.
    /// </summary>
    [HttpGet("revenue/monthly")]
    public async Task<ActionResult<List<RevenueMetricMonthlyResponse>>> GetRevenueMonthly(
        [FromQuery] Guid propertyId,
        [FromQuery] int year,
        CancellationToken ct)
    {
        var metrics = await _revenueMetrics.GetMonthlyMetricsAsync(propertyId, year, ct);

        var response = metrics.Select(m => new RevenueMetricMonthlyResponse(
            m.Id,
            m.PropertyId,
            m.Year,
            m.Month,
            m.RentCollected,
            m.Expenses,
            m.NetRevenue
        )).ToList();

        return Ok(response);
    }

    /// <summary>
    /// Get top properties by booking count within a date range.
    /// </summary>
    [HttpGet("top/properties")]
    public async Task<ActionResult<List<TopPropertyResponse>>> GetTopProperties(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] int take = 10,
        CancellationToken ct = default)
    {
        var topProperties = await _bookingMetrics.GetTopPropertiesAsync(from, to, take, ct);

        var response = topProperties.Select((p, index) => new TopPropertyResponse(
            p.PropertyId,
            p.TotalBookings,
            index + 1
        )).ToList();

        return Ok(response);
    }
}
