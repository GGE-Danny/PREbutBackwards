using AccountingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountingService.Infrastructure.Persistence.Configurations;

public sealed class UnitRateConfiguration : IEntityTypeConfiguration<UnitRate>
{
    public void Configure(EntityTypeBuilder<UnitRate> builder)
    {
        builder.ToTable("unit_rates");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Rate).HasPrecision(18, 2);

        builder.HasIndex(x => x.PropertyId);
        builder.HasIndex(x => x.UnitId);
        builder.HasIndex(x => x.IsActive);

        // Composite index for efficient lookups
        builder.HasIndex(x => new { x.UnitId, x.IsActive, x.EffectiveFrom, x.EffectiveTo });

        // Unique active rate per unit (only one active rate at a time)
        // Note: Postgres partial unique index would be ideal, but EF doesn't support directly
        // Business logic will enforce single active rate per unit
        builder.HasIndex(x => new { x.UnitId, x.IsActive })
               .HasFilter("\"IsActive\" = true AND \"DeletedAt\" IS NULL")
               .IsUnique();
    }
}
