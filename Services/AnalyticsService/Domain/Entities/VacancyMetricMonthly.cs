using AnalyticsService.Domain.Common;

namespace AnalyticsService.Domain.Entities;

/// <summary>
/// Monthly vacancy metrics per property/unit.
/// AvailableDays = 30 for MVP (simplified).
/// </summary>
public class VacancyMetricMonthly : BaseEntity
{
    public Guid PropertyId { get; set; }
    public Guid UnitId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int OccupiedDays { get; set; }
    public int AvailableDays { get; set; } = 30; // MVP: fixed at 30
}
