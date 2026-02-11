using AccountingService.Infrastructure.Consumers.Contracts;
using BookingService.Application.Dtos.Requests;
using BookingService.Application.Dtos.Responses;
using BookingService.Domain.Entities;
using BookingService.Domain.Enums;
using BookingService.Infrastructure.Clients;
using BookingService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Api.Controllers;

[Route("api/v1/bookings")]
public sealed class BookingsController : ApiControllerBase
{
    private readonly BookingDbContext _db;
    private readonly IPropertyServiceClient _propertyClient;
    private readonly IPublishEndpoint _publish;

    public BookingsController(
        BookingDbContext db,
        IPropertyServiceClient propertyClient,
        IPublishEndpoint publish)
    {
        _db = db;
        _propertyClient = propertyClient;
        _publish = publish;
    }

    [HttpPost]
    [Authorize(Policy = "booking.write")]
    public async Task<ActionResult<BookingResponse>> Create([FromBody] CreateBookingRequest req, CancellationToken ct)
    {
        if (!TryGetCallerUserId(out var callerUserId))
            return Unauthorized();

        // Tenant cannot create for someone else
        var tenantUserId = IsTenant(User)
            ? callerUserId
            : (req.TenantUserId ?? Guid.Empty);

        if (tenantUserId == Guid.Empty)
            return BadRequest("tenantUserId is required for non-tenant callers.");

        if (IsTenant(User) && tenantUserId != callerUserId)
            return Forbid();

        if (req.EndDate < req.StartDate)
            return BadRequest("endDate must be >= startDate.");

        // Validate unit belongs to property if unitId provided
        if (req.UnitId.HasValue)
        {
            var ok = await _propertyClient.UnitBelongsToPropertyAsync(req.PropertyId, req.UnitId.Value, ct);
            if (!ok) return BadRequest("Unit does not belong to the property.");
        }

        if (req.UnitId.HasValue)
        {
            var conflictId = await FindConfirmedConflictBookingId(
                req.PropertyId,
                req.UnitId.Value,
                req.StartDate,
                req.EndDate,
                excludeBookingId: null,
                ct);

            if (conflictId is not null)
                return Conflict(new { message = "Unit is not available for the selected date range.", conflictingBookingId = conflictId });
        }

        // Idempotency (pragmatic): if a Pending booking exists with same tenant/property/unit and same dates, return it
        var existing = await _db.Bookings
            .AsNoTracking()
            .Where(x => x.TenantUserId == tenantUserId
                        && x.PropertyId == req.PropertyId
                        && x.UnitId == req.UnitId
                        && x.StartDate == req.StartDate
                        && x.EndDate == req.EndDate
                        && x.Status == BookingStatus.Pending)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (existing is not null)
            return Ok(ToResponse(existing));

        var booking = new Booking
        {
            TenantUserId = tenantUserId,
            PropertyId = req.PropertyId,
            UnitId = req.UnitId,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            Notes = req.Notes,
            Status = BookingStatus.Pending
        };

        var log = new BookingLog
        {
            BookingId = booking.Id,
            EventType = BookingLogEventType.Created,
            Notes = req.Notes,
            ActorUserId = callerUserId
        };

        _db.Bookings.Add(booking);
        _db.BookingLogs.Add(log);

        await _db.SaveChangesAsync(ct);

        return Ok(ToResponse(booking));
    }

    [HttpGet("{bookingId:guid}")]
    [Authorize(Policy = "booking.read")]
    public async Task<ActionResult<BookingResponse>> GetById([FromRoute] Guid bookingId, CancellationToken ct)
    {
        if (!TryGetCallerUserId(out _))
            return Unauthorized();

        var booking = await _db.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == bookingId, ct);

        if (booking is null)
            return NotFound();

        // Tenant-scoped access
        if (!CanAccessTenant(booking.TenantUserId))
            return Forbid();

        return Ok(ToResponse(booking));
    }

    [HttpPatch("{bookingId:guid}/status")]
    [Authorize(Policy = "booking.write")]
    public async Task<ActionResult<BookingResponse>> UpdateStatus(
        [FromRoute] Guid bookingId,
        [FromBody] UpdateBookingStatusRequest req,
        CancellationToken ct)
    {
        if (!TryGetCallerUserId(out var actorId))
            return Unauthorized();

        var booking = await _db.Bookings
            .FirstOrDefaultAsync(x => x.Id == bookingId, ct);

        if (booking is null)
            return NotFound();

        if (!CanAccessTenant(booking.TenantUserId))
            return Forbid();

        if (req.Status == BookingStatus.Confirmed)
        {
            if (!booking.UnitId.HasValue)
                return BadRequest("Cannot confirm availability without UnitId.");

            var conflictId = await FindConfirmedConflictBookingId(
                booking.PropertyId,
                booking.UnitId.Value,
                booking.StartDate,
                booking.EndDate,
                excludeBookingId: booking.Id,
                ct);

            if (conflictId is not null)
                return Conflict(new { message = "Unit is not available for the selected date range.", conflictingBookingId = conflictId });
        }

        // Capture previous status BEFORE transition
        var previousStatus = booking.Status;

        // State transition rules
        var isTenant = IsTenant(User);
        if (isTenant)
        {
            // Tenant can only cancel (typically Pending or Confirmed)
            if (req.Status != BookingStatus.Cancelled)
                return Forbid();

            if (booking.Status is BookingStatus.Completed or BookingStatus.Cancelled)
                return BadRequest("Booking is already finalized.");

            booking.Status = BookingStatus.Cancelled;
        }
        else
        {
            // Staff manage transitions
            if (!CanManage(User))
                return Forbid();

            if (!IsValidStaffTransition(booking.Status, req.Status))
                return BadRequest($"Invalid status transition: {booking.Status} -> {req.Status}");

            booking.Status = req.Status;
        }

        if (!string.IsNullOrWhiteSpace(req.Notes))
            booking.Notes = req.Notes;

        booking.UpdatedAt = DateTime.UtcNow;

        _db.BookingLogs.Add(new BookingLog
        {
            BookingId = booking.Id,
            EventType = req.Status switch
            {
                BookingStatus.Confirmed => BookingLogEventType.Confirmed,
                BookingStatus.Cancelled => BookingLogEventType.Cancelled,
                BookingStatus.Completed => BookingLogEventType.Completed,
                _ => BookingLogEventType.NotesUpdated
            },
            Notes = req.Notes,
            ActorUserId = actorId
        });

        await _db.SaveChangesAsync(ct);

        // Publish BookingConfirmedEvent ONLY when transitioning TO Confirmed
        if (previousStatus != BookingStatus.Confirmed &&
            booking.Status == BookingStatus.Confirmed)
        {
            if (!booking.UnitId.HasValue)
                throw new InvalidOperationException("Confirmed booking must have UnitId.");

            await _publish.Publish(new BookingConfirmedEvent
            {
                BookingId = booking.Id,
                TenantUserId = booking.TenantUserId,
                PropertyId = booking.PropertyId,
                UnitId = booking.UnitId.Value,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate
            }, ct);
        }

        // return DTO from tracked entity
        return Ok(ToResponse(booking));
    }

    [HttpDelete("{bookingId:guid}")]
    [Authorize(Policy = "booking.manage")]
    public async Task<IActionResult> SoftDelete([FromRoute] Guid bookingId, CancellationToken ct)
    {
        if (!TryGetCallerUserId(out var actorId))
            return Unauthorized();

        if (!CanManage(User))
            return Forbid();

        var booking = await _db.Bookings.FirstOrDefaultAsync(x => x.Id == bookingId, ct);
        if (booking is null) return NotFound();

        booking.DeletedAt = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;

        _db.BookingLogs.Add(new BookingLog
        {
            BookingId = booking.Id,
            EventType = BookingLogEventType.NotesUpdated,
            Notes = "Soft deleted",
            ActorUserId = actorId
        });

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    private async Task<Guid?> FindConfirmedConflictBookingId(
    Guid propertyId,
    Guid unitId,
    DateOnly start,
    DateOnly end,
    Guid? excludeBookingId,
    CancellationToken ct)
    {
        var q = _db.Bookings
            .AsNoTracking()
            .Where(b =>
                b.PropertyId == propertyId &&
                b.UnitId == unitId &&
                b.Status == BookingStatus.Confirmed &&
                b.StartDate <= end &&
                start <= b.EndDate
            );

        if (excludeBookingId.HasValue)
            q = q.Where(b => b.Id != excludeBookingId.Value);

        return await q
            .Select(b => (Guid?)b.Id)
            .FirstOrDefaultAsync(ct);
    }



    private static bool IsValidStaffTransition(BookingStatus from, BookingStatus to)
    {
        return (from, to) switch
        {
            (BookingStatus.Pending, BookingStatus.Confirmed) => true,
            (BookingStatus.Pending, BookingStatus.Cancelled) => true,
            (BookingStatus.Confirmed, BookingStatus.Cancelled) => true,
            (BookingStatus.Confirmed, BookingStatus.Completed) => true,
            _ => false
        };
    }

    private static BookingResponse ToResponse(Booking b) => new(
        b.Id,
        b.TenantUserId,
        b.PropertyId,
        b.UnitId,
        b.StartDate,
        b.EndDate,
        b.Status,
        b.Notes,
        b.CreatedAt,
        b.UpdatedAt
    );
}
