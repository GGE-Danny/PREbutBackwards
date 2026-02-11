using SupportService.Domain.Enums;

namespace SupportService.Application.Dtos.Responses;

public record TicketMessageResponse(
    Guid Id,
    Guid TicketId,
    TicketMessageType MessageType,
    string Body,
    Guid ActorUserId,
    DateTime CreatedAt
);
