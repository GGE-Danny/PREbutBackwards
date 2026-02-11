using AccountingService.Domain.Enums;

namespace AccountingService.Application.Dtos.Responses;

public sealed record PaymentResponse(
    Guid Id,
    Guid InvoiceId,
    decimal Amount,
    PaymentMethod PaymentMethod,
    string ReferenceId,
    DateTime CreatedAt
);
