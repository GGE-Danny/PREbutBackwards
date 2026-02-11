using BookingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingService.Infrastructure.Persistence.Configurations;

public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.HasIndex(x => x.TenantUserId);
        builder.HasIndex(x => x.PropertyId);
        builder.HasIndex(x => x.UnitId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatedAt);

        // Helpful composite index for tenant booking history filtering
        builder.HasIndex(x => new { x.TenantUserId, x.Status, x.CreatedAt });

        builder.HasIndex(x => new { x.PropertyId, x.UnitId, x.Status, x.StartDate, x.EndDate });


        builder.HasMany(x => x.Logs)
               .WithOne(x => x.Booking)
               .HasForeignKey(x => x.BookingId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
