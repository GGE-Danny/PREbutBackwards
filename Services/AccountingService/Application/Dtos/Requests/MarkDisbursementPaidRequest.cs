namespace AccountingService.Application.Dtos.Requests;

public sealed record MarkDisbursementPaidRequest(
    bool IsPaid,
    string? Notes
);
