using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SalesService.Domain.Entities;

namespace SalesService.Infrastructure.Persistence.Configurations;

public class CommissionRecordConfiguration : IEntityTypeConfiguration<CommissionRecord>
{
    public void Configure(EntityTypeBuilder<CommissionRecord> builder)
    {
        builder.ToTable("commission_records");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.CommissionPercent).HasPrecision(5, 2);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.HasIndex(x => x.OwnerId);
        builder.HasIndex(x => x.PropertyId);
        builder.HasIndex(x => x.BookingId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.EarnedAt);

        // Unique constraint on BookingId where not null
        builder.HasIndex(x => x.BookingId)
            .IsUnique()
            .HasFilter("\"BookingId\" IS NOT NULL");
    }
}
