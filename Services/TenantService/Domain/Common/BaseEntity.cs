using System.ComponentModel.DataAnnotations;

namespace TenantService.Domain.Common;
// (same file can live in PropertyService.Domain.Common)

public abstract class BaseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Soft delete
    public DateTime? DeletedAt { get; set; }
}
