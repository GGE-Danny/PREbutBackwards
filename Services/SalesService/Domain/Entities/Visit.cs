using SalesService.Domain.Common;
using SalesService.Domain.Enums;

namespace SalesService.Domain.Entities;

public class Visit : BaseEntity
{
    public Guid LeadId { get; set; }
    public Guid PropertyId { get; set; }
    public Guid? UnitId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public VisitOutcome Outcome { get; set; } = VisitOutcome.Pending;
    public string? Notes { get; set; }
    public Guid ActorUserId { get; set; }

    // Navigation
    public Lead Lead { get; set; } = null!;
}
