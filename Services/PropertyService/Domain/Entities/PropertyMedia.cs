using PropertyService.Domain.Common;

namespace PropertyService.Domain.Entities;

public class PropertyMedia : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid PropertyId { get; set; }
    public Property Property { get; set; } = default!;

    public string MediaType { get; set; } = "Image"; // Image/Document
    public string DocumentId { get; set; } = default!; // Document Service id
    public string? Title { get; set; }
    public string? TagsCsv { get; set; } // simple storage for now
    public Guid? UploadedByUserId { get; set; }

}