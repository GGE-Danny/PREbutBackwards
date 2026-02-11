using AccountingService.Domain.Common;
using AccountingService.Domain.Enums;

namespace AccountingService.Domain.Entities;

public class Invoice(
    Guid bookingId,
    Guid tenantUserId,
    Guid propertyId,
    decimal amount,
    DateOnly dueDate,
    PaymentStatus status,
    InvoiceType type
) : BaseEntity
{
    public Guid BookingId { get; set; } = bookingId;
    public Guid TenantUserId { get; set; } = tenantUserId;
    public Guid PropertyId { get; set; } = propertyId;

    public decimal Amount { get; set; } = amount;
    public DateOnly DueDate { get; set; } = dueDate;

    public PaymentStatus Status { get; set; } = status;
    public InvoiceType Type { get; set; } = type;

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
