using BookingService.Domain.Common;
using BookingService.Domain.Enums;

namespace BookingService.Domain.Entities;

public class BookingLog : BaseEntity
{
    public Guid BookingId { get; set; }
    public Booking Booking { get; set; } = default!;

    public BookingLogEventType EventType { get; set; }
    public string? Notes { get; set; }
    public Guid? ActorUserId { get; set; }
}
