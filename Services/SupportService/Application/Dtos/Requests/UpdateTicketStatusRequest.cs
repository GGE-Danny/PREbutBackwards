using SupportService.Domain.Enums;

namespace SupportService.Application.Dtos.Requests;

public record UpdateTicketStatusRequest(
    TicketStatus Status,
    string? Notes
);
