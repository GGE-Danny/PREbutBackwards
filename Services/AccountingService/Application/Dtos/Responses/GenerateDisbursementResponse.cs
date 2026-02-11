namespace AccountingService.Application.Dtos.Responses;

public sealed record GenerateDisbursementResponse(
    OwnerDisbursementResponse Disbursement,
    DisbursementCalculationBreakdownResponse Breakdown
);
