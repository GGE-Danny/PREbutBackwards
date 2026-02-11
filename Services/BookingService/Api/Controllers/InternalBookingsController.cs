using BookingService.Application.Dtos.Requests;
using BookingService.Domain.Entities;
using BookingService.Domain.Enums;
using BookingService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Api.Controllers;

[Route("api/v1/internal")]
public sealed class InternalBookingsController : ApiControllerBase
{
    private readonly BookingDbContext _db;
    public InternalBookingsController(BookingDbContext db) => _db = db;

    [HttpPost("booking-created")]
    [Authorize(Policy = "booking.internal.write")]
    public async Task<IActionResult> BookingCreated([FromBody] InternalBookingEventRequest req, CancellationToken ct)
    {
        // Idempotent: if booking exists, just log (or no-op)
        var exists = await _db.Bookings.AnyAsync(x => x.Id == req.BookingId, ct);
        if (!exists)
        {
            // If other services create booking IDs (rare), you can decide to reject instead.
            return NotFound("Booking not found.");
        }

        _db.BookingLogs.Add(new BookingLog
        {
            BookingId = req.BookingId,
            EventType = BookingLogEventType.Created,
            Notes = req.Notes,
            ActorUserId = req.ActorUserId
        });

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("booking-cancelled")]
    [Authorize(Policy = "booking.internal.write")]
    public async Task<IActionResult> BookingCancelled([FromBody] InternalBookingEventRequest req, CancellationToken ct)
    {
        var booking = await _db.Bookings.FirstOrDefaultAsync(x => x.Id == req.BookingId, ct);
        if (booking is null) return NotFound();

        if (booking.Status is BookingStatus.Cancelled or BookingStatus.Completed)
            return NoContent(); // idempotent

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;

        _db.BookingLogs.Add(new BookingLog
        {
            BookingId = req.BookingId,
            EventType = BookingLogEventType.Cancelled,
            Notes = req.Notes,
            ActorUserId = req.ActorUserId
        });

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
