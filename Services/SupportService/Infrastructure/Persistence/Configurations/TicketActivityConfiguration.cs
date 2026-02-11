using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportService.Domain.Entities;

namespace SupportService.Infrastructure.Persistence.Configurations;

public class TicketActivityConfiguration : IEntityTypeConfiguration<TicketActivity>
{
    public void Configure(EntityTypeBuilder<TicketActivity> builder)
    {
        builder.ToTable("ticket_activities");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Event).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.HasIndex(x => x.TicketId);
        builder.HasIndex(x => x.OccurredAt);
    }
}
