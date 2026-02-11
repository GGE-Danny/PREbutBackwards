using SupportService.Domain.Common;
using SupportService.Domain.Enums;

namespace SupportService.Domain.Entities;

public class Ticket : BaseEntity
{
    public Guid? TenantUserId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string Subject { get; set; } = null!;
    public string Description { get; set; } = null!;
    public TicketCategory Category { get; set; }
    public TicketPriority Priority { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public Guid? PropertyId { get; set; }
    public Guid? BookingId { get; set; }

    // Navigation
    public ICollection<TicketMessage> Messages { get; set; } = new List<TicketMessage>();
    public ICollection<TicketActivity> Activities { get; set; } = new List<TicketActivity>();
}
