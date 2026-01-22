using PropertyService.Domain.Common;
using PropertyService.Domain.Enums;

namespace PropertyService.Domain.Entities;

public class Property : BaseEntity
{
    // REMOVE:
    // public Guid TenantId { get; set; }

    public string? PropertyCode { get; set; }
    public string Name { get; set; } = default!;
    public PropertyType Type { get; set; }
    public PropertyStatus Status { get; set; } = PropertyStatus.Draft;

    // change to Guid?
    public Guid? OwnerId { get; set; }   // Auth userId (sub)

    // snapshots optional (keep if you want)
    public string? OwnerNameSnapshot { get; set; }
    public string? OwnerPhoneSnapshot { get; set; }
    public string? OwnerEmailSnapshot { get; set; }

    public string? AddressLine { get; set; }
    public string? Area { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? Landmark { get; set; }

    public decimal? GpsLatitude { get; set; }
    public decimal? GpsLongitude { get; set; }

    public string? Notes { get; set; }

    public List<Unit> Units { get; set; } = new();
    public List<PropertyUtility> Utilities { get; set; } = new();
    public List<PropertyMedia> Media { get; set; } = new();
    public List<PropertyTimelineEvent> Timeline { get; set; } = new();
}

