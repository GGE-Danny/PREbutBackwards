namespace SupportService.Application.Dtos.Responses;

public record TicketActivityResponse(
    Guid Id,
    Guid TicketId,
    string Event,
    string? Notes,
    Guid? ActorUserId,
    DateTime OccurredAt
);
