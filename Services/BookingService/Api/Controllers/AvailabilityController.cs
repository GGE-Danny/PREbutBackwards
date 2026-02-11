using BookingService.Application.Dtos.Responses;
using BookingService.Domain.Enums;
using BookingService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Api.Controllers;

[Route("api/v1/availability")]
public sealed class AvailabilityController : ApiControllerBase
{
    private readonly BookingDbContext _db;

    public AvailabilityController(BookingDbContext db) => _db = db;

    [HttpGet]
    [Authorize(Policy = "booking.read")]
    public async Task<ActionResult<AvailabilityResponse>> Check(
        [FromQuery] Guid propertyId,
        [FromQuery] Guid unitId,
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized();

        if (endDate < startDate)
            return BadRequest("endDate must be >= startDate.");

        var conflictId = await FindConfirmedConflictBookingId(propertyId, unitId, startDate, endDate, excludeBookingId: null, ct);

        return Ok(new AvailabilityResponse(
            propertyId,
            unitId,
            startDate,
            endDate,
            IsAvailable: conflictId is null,
            ConflictingBookingId: conflictId
        ));
    }

    private async Task<Guid?> FindConfirmedConflictBookingId(
        Guid propertyId,
        Guid unitId,
        DateOnly start,
        DateOnly end,
        Guid? excludeBookingId,
        CancellationToken ct)
    {
        // Overlap rule: start < existing.EndDate AND existing.StartDate < end
        // (works well for “ranges”, avoids off-by-one issues)
        var q = _db.Bookings.AsNoTracking().Where(b =>
            b.PropertyId == propertyId &&
            b.UnitId == unitId &&
            b.Status == BookingStatus.Confirmed &&
           b.StartDate <= end && start <= b.EndDate
);

        if (excludeBookingId.HasValue)
            q = q.Where(b => b.Id != excludeBookingId.Value);

        return await q.Select(b => (Guid?)b.Id).FirstOrDefaultAsync(ct);
    }
}
