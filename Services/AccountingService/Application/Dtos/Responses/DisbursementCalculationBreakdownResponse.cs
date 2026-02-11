namespace AccountingService.Application.Dtos.Responses;

public sealed record DisbursementCalculationBreakdownResponse(
    decimal RentCollected,
    decimal Expenses,
    decimal CommissionPercent,
    decimal CommissionAmount,
    decimal DisbursementAmount
);
