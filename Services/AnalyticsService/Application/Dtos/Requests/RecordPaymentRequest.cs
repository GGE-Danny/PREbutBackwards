namespace AnalyticsService.Application.Dtos.Requests;

/// <summary>
/// Internal endpoint request to record a payment (if AccountingService doesn't publish events yet).
/// </summary>
public record RecordPaymentRequest(
    Guid InvoiceId,
    Guid PropertyId,
    Guid TenantUserId,
    decimal Amount,
    DateTime PaidAtUtc,
    string Type // "Rent" or "ServiceFee"
);
