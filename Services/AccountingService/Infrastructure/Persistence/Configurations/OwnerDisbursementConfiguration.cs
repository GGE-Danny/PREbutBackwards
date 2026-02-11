using AccountingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountingService.Infrastructure.Persistence.Configurations;

public sealed class OwnerDisbursementConfiguration : IEntityTypeConfiguration<OwnerDisbursement>
{
    public void Configure(EntityTypeBuilder<OwnerDisbursement> builder)
    {
        builder.ToTable("owner_disbursements");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount).HasPrecision(18, 2);

        builder.HasIndex(x => x.OwnerId);
        builder.HasIndex(x => x.PropertyId);
        builder.HasIndex(x => x.IsPaid);

        // Unique constraint for idempotency: one disbursement per owner+property+period
        builder.HasIndex(x => new { x.OwnerId, x.PropertyId, x.PeriodStart, x.PeriodEnd })
               .IsUnique()
               .HasFilter("\"DeletedAt\" IS NULL");
    }
}
