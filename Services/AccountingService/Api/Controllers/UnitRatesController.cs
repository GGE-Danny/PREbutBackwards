using AccountingService.Application.Dtos.Requests;
using AccountingService.Application.Dtos.Responses;
using AccountingService.Application.Interfaces;
using AccountingService.Domain.Entities;
using AccountingService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountingService.Api.Controllers;

[Route("api/v1/unit-rates")]
[Authorize(Policy = "accounting.manage")]
public sealed class UnitRatesController : ApiControllerBase
{
    private readonly IUnitRateRepository _unitRates;
    private readonly ILedgerRepository _ledger;
    private readonly IUnitOfWork _uow;

    public UnitRatesController(
        IUnitRateRepository unitRates,
        ILedgerRepository ledger,
        IUnitOfWork uow)
    {
        _unitRates = unitRates;
        _ledger = ledger;
        _uow = uow;
    }

    /// <summary>
    /// Create a new unit rate. Auto-deactivates any existing active rate for the unit.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UnitRateResponse>> Create(
        [FromBody] CreateUnitRateRequest req,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out var actorId))
            return Unauthorized();

        if (!CanManage(User))
            return Forbid();

        if (req.Rate <= 0)
            return BadRequest("Rate must be > 0.");

        await _uow.BeginTransactionAsync(ct);

        try
        {
            // Get current active rate for audit
            var oldRate = await _unitRates.GetActiveRateForUnitAsync(req.UnitId, null, ct);
            decimal? oldRateValue = oldRate?.Rate;

            // Deactivate any existing active rate
            var yesterday = req.EffectiveFrom.AddDays(-1);
            await _unitRates.DeactivateActiveRateAsync(req.UnitId, yesterday, ct);

            // Create new active rate
            var unitRate = new UnitRate(
                propertyId: req.PropertyId,
                unitId: req.UnitId,
                rate: req.Rate,
                effectiveFrom: req.EffectiveFrom,
                effectiveTo: null,
                isActive: true
            );

            await _unitRates.AddAsync(unitRate, ct);

            // Ledger audit entry
            var notes = oldRateValue.HasValue
                ? $"Rate changed: {oldRateValue.Value} -> {req.Rate}. UnitId:{req.UnitId}. Actor:{actorId}"
                : $"Rate created: {req.Rate}. UnitId:{req.UnitId}. Actor:{actorId}";

            await _ledger.AddAsync(new LedgerEntry(
                entryType: LedgerEntryType.RateChange,
                amount: 0m,
                invoiceId: null,
                paymentId: null,
                expenseId: null,
                ownerDisbursementId: null,
                notes: notes
            ), ct);

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitAsync(ct);

            return Ok(ToResponse(unitRate));
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }

    /// <summary>
    /// List all rates (active + inactive) for a unit.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<UnitRateResponse>>> GetByUnit(
        [FromQuery] Guid unitId,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized();

        if (!CanManage(User))
            return Forbid();

        var rates = await _unitRates.GetByUnitAsync(unitId, ct);
        return Ok(rates.Select(ToResponse).ToList());
    }

    /// <summary>
    /// Activate a historical rate (deactivates current active rate).
    /// </summary>
    [HttpPatch("{id:guid}/activate")]
    public async Task<ActionResult<UnitRateResponse>> Activate(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out var actorId))
            return Unauthorized();

        if (!CanManage(User))
            return Forbid();

        var rate = await _unitRates.GetByIdForUpdateAsync(id, ct);
        if (rate is null)
            return NotFound();

        if (rate.IsActive)
            return BadRequest("Rate is already active.");

        await _uow.BeginTransactionAsync(ct);

        try
        {
            // Get current active rate for audit
            var oldActiveRate = await _unitRates.GetActiveRateForUnitAsync(rate.UnitId, null, ct);
            decimal? oldRateValue = oldActiveRate?.Rate;

            // Deactivate current active rate
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            await _unitRates.DeactivateActiveRateAsync(rate.UnitId, today, ct);

            // Activate selected rate
            rate.IsActive = true;
            rate.EffectiveFrom = today.AddDays(1);
            rate.EffectiveTo = null;
            rate.UpdatedAt = DateTime.UtcNow;

            // Ledger audit entry
            var notes = oldRateValue.HasValue
                ? $"Rate activated: {oldRateValue.Value} -> {rate.Rate}. UnitId:{rate.UnitId}. Actor:{actorId}"
                : $"Rate activated: {rate.Rate}. UnitId:{rate.UnitId}. Actor:{actorId}";

            await _ledger.AddAsync(new LedgerEntry(
                entryType: LedgerEntryType.RateChange,
                amount: 0m,
                invoiceId: null,
                paymentId: null,
                expenseId: null,
                ownerDisbursementId: null,
                notes: notes
            ), ct);

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitAsync(ct);

            return Ok(ToResponse(rate));
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }

    /// <summary>
    /// Soft delete a rate. Active rates cannot be deleted (must deactivate first).
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out var actorId))
            return Unauthorized();

        if (!CanManage(User))
            return Forbid();

        var rate = await _unitRates.GetByIdForUpdateAsync(id, ct);
        if (rate is null)
            return NotFound();

        if (rate.IsActive)
            return BadRequest("Cannot delete active rate. Deactivate it first by activating another rate.");

        await _uow.BeginTransactionAsync(ct);

        try
        {
            rate.DeletedAt = DateTime.UtcNow;
            rate.UpdatedAt = DateTime.UtcNow;

            // Ledger audit entry
            await _ledger.AddAsync(new LedgerEntry(
                entryType: LedgerEntryType.RateChange,
                amount: 0m,
                invoiceId: null,
                paymentId: null,
                expenseId: null,
                ownerDisbursementId: null,
                notes: $"Rate deleted: {rate.Rate}. UnitId:{rate.UnitId}. Actor:{actorId}"
            ), ct);

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitAsync(ct);

            return NoContent();
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }

    private static UnitRateResponse ToResponse(UnitRate r) => new(
        r.Id,
        r.PropertyId,
        r.UnitId,
        r.Rate,
        r.IsActive,
        r.EffectiveFrom,
        r.EffectiveTo,
        r.CreatedAt
    );
}
