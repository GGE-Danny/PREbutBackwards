namespace SupportService.Application.Dtos.Requests;

public record AssignTicketRequest(
    Guid AssignedToUserId
);
