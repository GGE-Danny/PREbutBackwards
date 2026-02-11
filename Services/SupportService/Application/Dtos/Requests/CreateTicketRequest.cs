using SupportService.Domain.Enums;

namespace SupportService.Application.Dtos.Requests;

public record CreateTicketRequest(
    string Subject,
    string Description,
    TicketCategory Category,
    TicketPriority Priority,
    Guid? PropertyId,
    Guid? BookingId
);
