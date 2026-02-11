using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportService.Domain.Entities;

namespace SupportService.Infrastructure.Persistence.Configurations;

public class TicketMessageConfiguration : IEntityTypeConfiguration<TicketMessage>
{
    public void Configure(EntityTypeBuilder<TicketMessage> builder)
    {
        builder.ToTable("ticket_messages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Body).IsRequired().HasMaxLength(4000);

        builder.HasIndex(x => x.TicketId);
        builder.HasIndex(x => x.CreatedAt);
    }
}
