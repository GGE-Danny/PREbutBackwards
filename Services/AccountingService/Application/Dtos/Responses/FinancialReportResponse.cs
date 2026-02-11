namespace AccountingService.Application.Dtos.Responses;

public sealed record FinancialReportResponse(
    string Period,              // Monthly / Quarterly
    DateOnly FromDate,
    DateOnly ToDate,
    decimal TotalRevenue,
    decimal TotalExpenses,
    decimal NetRevenue
);
