using AccountingService.Domain.Common;
using AccountingService.Domain.Enums;

namespace AccountingService.Domain.Entities;

public class Payment(
    Guid invoiceId,
    decimal amount,
    PaymentMethod paymentMethod,
    string referenceId
) : BaseEntity
{
    public Guid InvoiceId { get; set; } = invoiceId;
    public Invoice Invoice { get; set; } = default!;

    public decimal Amount { get; set; } = amount;
    public PaymentMethod PaymentMethod { get; set; } = paymentMethod;
    public string ReferenceId { get; set; } = referenceId;
}
