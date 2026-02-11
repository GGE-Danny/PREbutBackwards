using AnalyticsService.Domain.Common;

namespace AnalyticsService.Domain.Entities;

/// <summary>
/// Monthly revenue metrics per property.
/// </summary>
public class RevenueMetricMonthly : BaseEntity
{
    public Guid PropertyId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal RentCollected { get; set; }
    public decimal Expenses { get; set; }
    public decimal NetRevenue { get; set; } // RentCollected - Expenses
}
