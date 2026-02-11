using AccountingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountingService.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.ReferenceId).HasMaxLength(200);

        builder.HasIndex(x => x.InvoiceId);
        builder.HasIndex(x => x.PaymentMethod);
        builder.HasIndex(x => x.CreatedAt);

        // Unique constraint on ReferenceId for idempotency
        builder.HasIndex(x => x.ReferenceId)
               .IsUnique();
    }
}
