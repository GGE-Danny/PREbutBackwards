namespace AccountingService.Application.Interfaces;

public interface ICommissionCalculator
{
    decimal CalculateDisbursementAmount(decimal rentCollected, decimal expenses, decimal commissionPercent);
}
