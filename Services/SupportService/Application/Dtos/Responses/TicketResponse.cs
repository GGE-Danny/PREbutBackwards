using SupportService.Domain.Enums;

namespace SupportService.Application.Dtos.Responses;

public record TicketResponse(
    Guid Id,
    Guid? TenantUserId,
    Guid CreatedByUserId,
    Guid? AssignedToUserId,
    string Subject,
    string Description,
    TicketCategory Category,
    TicketPriority Priority,
    TicketStatus Status,
    Guid? PropertyId,
    Guid? BookingId,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
