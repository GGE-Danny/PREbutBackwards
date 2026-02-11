using AccountingService.Domain.Entities;

namespace AccountingService.Application.Interfaces;

public interface IUnitRateRepository
{
    /// <summary>
    /// Gets the active rate for a unit, optionally checking effective date range.
    /// </summary>
    Task<UnitRate?> GetActiveRateForUnitAsync(Guid unitId, DateOnly? asOfDate, CancellationToken ct);

    Task<UnitRate?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<UnitRate?> GetByIdForUpdateAsync(Guid id, CancellationToken ct);

    Task<List<UnitRate>> GetByUnitAsync(Guid unitId, CancellationToken ct);

    Task DeactivateActiveRateAsync(Guid unitId, DateOnly effectiveTo, CancellationToken ct);

    Task AddAsync(UnitRate unitRate, CancellationToken ct);
}
