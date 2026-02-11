using BookingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingService.Infrastructure.Persistence.Configurations;

public sealed class BookingLogConfiguration : IEntityTypeConfiguration<BookingLog>
{
    public void Configure(EntityTypeBuilder<BookingLog> builder)
    {
        builder.ToTable("booking_logs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.HasIndex(x => x.BookingId);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => new { x.BookingId, x.CreatedAt });
    }
}
