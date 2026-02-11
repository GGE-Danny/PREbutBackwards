using SupportService.Domain.Common;

namespace SupportService.Domain.Entities;

public class TicketActivity : BaseEntity
{
    public Guid TicketId { get; set; }
    public string Event { get; set; } = null!;
    public string? Notes { get; set; }
    public Guid? ActorUserId { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Ticket Ticket { get; set; } = null!;
}
