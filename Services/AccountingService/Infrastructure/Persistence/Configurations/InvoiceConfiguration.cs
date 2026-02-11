using AccountingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountingService.Infrastructure.Persistence.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount).HasPrecision(18, 2);

        builder.HasIndex(x => x.BookingId);
        builder.HasIndex(x => x.TenantUserId);
        builder.HasIndex(x => x.PropertyId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.DueDate);
        builder.HasIndex(x => new { x.TenantUserId, x.Status, x.DueDate });
        builder.HasIndex(x => new { x.PropertyId, x.Type, x.Status });

        // Typically one "rent invoice" per booking (MVP uniqueness)
        builder.HasIndex(x => new { x.BookingId, x.Type })
               .IsUnique();

        builder.HasMany(x => x.Payments)
               .WithOne(x => x.Invoice)
               .HasForeignKey(x => x.InvoiceId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
