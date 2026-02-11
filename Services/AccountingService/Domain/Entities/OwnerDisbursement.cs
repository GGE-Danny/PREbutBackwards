using AccountingService.Domain.Common;

namespace AccountingService.Domain.Entities;

public class OwnerDisbursement(
    Guid ownerId,
    Guid propertyId,
    decimal amount,
    DateOnly periodStart,
    DateOnly periodEnd,
    bool isPaid
) : BaseEntity
{
    public Guid OwnerId { get; set; } = ownerId;
    public Guid PropertyId { get; set; } = propertyId;

    public decimal Amount { get; set; } = amount;

    public DateOnly PeriodStart { get; set; } = periodStart;
    public DateOnly PeriodEnd { get; set; } = periodEnd;

    public bool IsPaid { get; set; } = isPaid;
}
