using Microsoft.EntityFrameworkCore;
using SupportService.Domain.Entities;

namespace SupportService.Infrastructure.Persistence;

public class SupportDbContext : DbContext
{
    public SupportDbContext(DbContextOptions<SupportDbContext> options) : base(options) { }

    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketMessage> TicketMessages => Set<TicketMessage>();
    public DbSet<TicketActivity> TicketActivities => Set<TicketActivity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SupportDbContext).Assembly);

        // Global soft-delete filters
        modelBuilder.Entity<Ticket>().HasQueryFilter(x => x.DeletedAt == null);
        modelBuilder.Entity<TicketMessage>().HasQueryFilter(x => x.DeletedAt == null);
        modelBuilder.Entity<TicketActivity>().HasQueryFilter(x => x.DeletedAt == null);

        base.OnModelCreating(modelBuilder);
    }
}
