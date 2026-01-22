using TenantService.Domain.Common;

namespace TenantService.Domain.Entities;

public class TenantResidencyHistory : BaseEntity
{
    public Guid TenantUserId { get; set; } // Auth user id

    public Guid PropertyId { get; set; }   // from PropertyService
    public Guid UnitId { get; set; }       // from PropertyService

    public DateOnly MoveInDate { get; set; }
    public DateOnly? MoveOutDate { get; set; }

    public string Source { get; set; } = "occupancy"; // occupancy/manual/import
    public string? Notes { get; set; }
}
