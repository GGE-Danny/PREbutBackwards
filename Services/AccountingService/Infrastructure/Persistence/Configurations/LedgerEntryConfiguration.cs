using AccountingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccountingService.Infrastructure.Persistence.Configurations;

public sealed class LedgerEntryConfiguration : IEntityTypeConfiguration<LedgerEntry>
{
    public void Configure(EntityTypeBuilder<LedgerEntry> builder)
    {
        builder.ToTable("ledger_entries");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.HasIndex(x => x.EntryType);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.InvoiceId);
        builder.HasIndex(x => x.PaymentId);
        builder.HasIndex(x => x.ExpenseId);
        builder.HasIndex(x => x.OwnerDisbursementId);
    }
}
