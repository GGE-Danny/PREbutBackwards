using AccountingService.Application.Interfaces;

namespace AccountingService.Infrastructure;

public sealed class CommissionCalculator : ICommissionCalculator
{
    public decimal CalculateDisbursementAmount(decimal rentCollected, decimal expenses, decimal commissionPercent)
    {
        if (commissionPercent < 0 || commissionPercent > 100)
            throw new ArgumentOutOfRangeException(nameof(commissionPercent));

        var commission = rentCollected * (commissionPercent / 100m);
        var net = rentCollected - expenses - commission;
        return net < 0 ? 0 : net;
    }
}
