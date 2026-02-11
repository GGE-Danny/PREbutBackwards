using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesService.Application.Dtos.Requests;
using SalesService.Application.Dtos.Responses;
using SalesService.Application.Interfaces;
using SalesService.Domain.Entities;

namespace SalesService.Api.Controllers;

[Route("api/v1/commissions")]
public sealed class CommissionsController : ApiControllerBase
{
    private readonly ICommissionRepository _commissions;
    private readonly IUnitOfWork _uow;

    public CommissionsController(ICommissionRepository commissions, IUnitOfWork uow)
    {
        _commissions = commissions;
        _uow = uow;
    }

    /// <summary>
    /// Create a commission record (sales.manage only).
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "sales.manage")]
    public async Task<ActionResult<CommissionRecordResponse>> Create(
        [FromBody] CreateCommissionRecordRequest req,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized();

        if (!CanManage(User))
            return Forbid();

        // Validation
        if (req.Amount <= 0)
            return BadRequest("Amount must be greater than 0.");

        if (req.CommissionPercent < 0 || req.CommissionPercent > 100)
            return BadRequest("CommissionPercent must be between 0 and 100.");

        // Check for duplicate BookingId
        if (req.BookingId.HasValue)
        {
            var existing = await _commissions.GetByBookingIdAsync(req.BookingId.Value, ct);
            if (existing is not null)
                return Conflict("Commission record already exists for this booking.");
        }

        // Ensure EarnedAt is UTC
        var earnedAtUtc = req.EarnedAtUtc.Kind == DateTimeKind.Utc
            ? req.EarnedAtUtc
            : DateTime.SpecifyKind(req.EarnedAtUtc, DateTimeKind.Utc);

        var record = new CommissionRecord
        {
            OwnerId = req.OwnerId,
            PropertyId = req.PropertyId,
            UnitId = req.UnitId,
            BookingId = req.BookingId,
            LeadId = req.LeadId,
            Amount = req.Amount,
            CommissionPercent = req.CommissionPercent,
            EarnedAt = earnedAtUtc,
            Notes = req.Notes
        };

        await _commissions.AddAsync(record, ct);
        await _uow.SaveChangesAsync(ct);

        return Ok(ToResponse(record));
    }

    /// <summary>
    /// Update commission status (sales.manage only).
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = "sales.manage")]
    public async Task<ActionResult<CommissionRecordResponse>> UpdateStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateCommissionStatusRequest req,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized();

        if (!CanManage(User))
            return Forbid();

        var record = await _commissions.GetByIdForUpdateAsync(id, ct);
        if (record is null)
            return NotFound();

        record.Status = req.Status;
        if (!string.IsNullOrWhiteSpace(req.Notes))
            record.Notes = req.Notes;
        record.UpdatedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync(ct);

        return Ok(ToResponse(record));
    }

    /// <summary>
    /// Query commission records (staff only).
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "sales.read")]
    public async Task<ActionResult<List<CommissionRecordResponse>>> Query(
        [FromQuery] Guid? propertyId,
        [FromQuery] Guid? ownerId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized();

        if (!CanManage(User))
            return Forbid();

        var records = await _commissions.QueryAsync(propertyId, ownerId, from, to, ct);
        return Ok(records.Select(ToResponse).ToList());
    }

    private static CommissionRecordResponse ToResponse(CommissionRecord r) => new(
        r.Id,
        r.OwnerId,
        r.PropertyId,
        r.UnitId,
        r.BookingId,
        r.LeadId,
        r.Amount,
        r.CommissionPercent,
        r.Status,
        r.EarnedAt,
        r.Notes,
        r.CreatedAt
    );
}
