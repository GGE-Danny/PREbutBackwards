using SupportService.Domain.Common;
using SupportService.Domain.Enums;

namespace SupportService.Domain.Entities;

public class TicketMessage : BaseEntity
{
    public Guid TicketId { get; set; }
    public TicketMessageType MessageType { get; set; }
    public string Body { get; set; } = null!;
    public Guid ActorUserId { get; set; }

    // Navigation
    public Ticket Ticket { get; set; } = null!;
}
