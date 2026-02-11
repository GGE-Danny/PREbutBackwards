using BookingService.Domain.Common;
using BookingService.Domain.Enums;

namespace BookingService.Domain.Entities;

public class Booking : BaseEntity
{
    public Guid TenantUserId { get; set; }
    public Guid PropertyId { get; set; }
    public Guid? UnitId { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public string? Notes { get; set; }

    public ICollection<BookingLog> Logs { get; set; } = new List<BookingLog>();
}
