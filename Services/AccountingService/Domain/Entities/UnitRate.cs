using AccountingService.Domain.Common;

namespace AccountingService.Domain.Entities;

/// <summary>
/// Pricing owned by AccountingService. Rate is monthly.
/// EndDate is inclusive across ERP (EffectiveTo inclusive).
/// </summary>
public class UnitRate(
    Guid propertyId,
    Guid unitId,
    decimal rate,
    DateOnly? effectiveFrom,
    DateOnly? effectiveTo,
    bool isActive
) : BaseEntity
{
    public Guid PropertyId { get; set; } = propertyId;
    public Guid UnitId { get; set; } = unitId;

    /// <summary>
    /// Monthly rate in base currency.
    /// </summary>
    public decimal Rate { get; set; } = rate;

    /// <summary>
    /// Optional: when this rate becomes effective (inclusive).
    /// </summary>
    public DateOnly? EffectiveFrom { get; set; } = effectiveFrom;

    /// <summary>
    /// Optional: when this rate expires (inclusive).
    /// </summary>
    public DateOnly? EffectiveTo { get; set; } = effectiveTo;

    public bool IsActive { get; set; } = isActive;
}
