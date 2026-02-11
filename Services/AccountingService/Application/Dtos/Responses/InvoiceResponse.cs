using AccountingService.Domain.Enums;

namespace AccountingService.Application.Dtos.Responses;

public sealed record InvoiceResponse(
    Guid Id,
    Guid BookingId,
    Guid TenantUserId,
    decimal Amount,
    DateOnly DueDate,
    PaymentStatus Status,
    InvoiceType Type,
    DateTime CreatedAt
);
