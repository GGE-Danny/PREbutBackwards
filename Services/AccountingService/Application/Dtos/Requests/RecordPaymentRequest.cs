using AccountingService.Domain.Enums;

namespace AccountingService.Application.Dtos.Requests;

public sealed record RecordPaymentRequest(
    Guid InvoiceId,
    decimal Amount,
    PaymentMethod PaymentMethod,
    string ReferenceId
);
