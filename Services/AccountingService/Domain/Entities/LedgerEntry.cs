using AccountingService.Domain.Common;
using AccountingService.Domain.Enums;

namespace AccountingService.Domain.Entities;

public sealed class LedgerEntry(
    LedgerEntryType entryType,
    decimal amount,
    Guid? invoiceId,
    Guid? paymentId,
    Guid? expenseId,
    Guid? ownerDisbursementId,
    string notes) : BaseEntity
{
    public LedgerEntryType EntryType { get; private set; } = entryType;
    public decimal Amount { get; private set; } = amount;

    public Guid? InvoiceId { get; private set; } = invoiceId;
    public Guid? PaymentId { get; private set; } = paymentId;
    public Guid? ExpenseId { get; private set; } = expenseId;
    public Guid? OwnerDisbursementId { get; private set; } = ownerDisbursementId;

    public string Notes { get; private set; } = notes;
}
