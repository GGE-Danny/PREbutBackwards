using SalesService.Domain.Common;
using SalesService.Domain.Enums;

namespace SalesService.Domain.Entities;

public class CommissionRecord : BaseEntity
{
    public Guid OwnerId { get; set; }
    public Guid PropertyId { get; set; }
    public Guid? UnitId { get; set; }
    public Guid? BookingId { get; set; }
    public Guid? LeadId { get; set; }
    public decimal Amount { get; set; }
    public decimal CommissionPercent { get; set; }
    public CommissionStatus Status { get; set; } = CommissionStatus.Pending;
    public string? Notes { get; set; }
    public DateTime EarnedAt { get; set; }
}
