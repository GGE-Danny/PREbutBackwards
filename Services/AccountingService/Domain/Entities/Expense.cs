using AccountingService.Domain.Common;
using AccountingService.Domain.Enums;

namespace AccountingService.Domain.Entities;

public class Expense(
    Guid propertyId,
    decimal amount,
    ExpenseCategory category,
    string description,
    DateTime incurredAt
) : BaseEntity
{
    public Guid PropertyId { get; set; } = propertyId;
    public decimal Amount { get; set; } = amount;

    public ExpenseCategory Category { get; set; } = category;
    public string Description { get; set; } = description;

    public DateTime IncurredAt { get; set; } = incurredAt;
}
