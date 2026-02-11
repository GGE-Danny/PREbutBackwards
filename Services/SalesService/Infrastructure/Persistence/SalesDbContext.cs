using Microsoft.EntityFrameworkCore;
using SalesService.Domain.Entities;

namespace SalesService.Infrastructure.Persistence;

public class SalesDbContext : DbContext
{
    public SalesDbContext(DbContextOptions<SalesDbContext> options) : base(options) { }

    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<CommissionRecord> CommissionRecords => Set<CommissionRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SalesDbContext).Assembly);

        // Global soft-delete filters
        modelBuilder.Entity<Lead>().HasQueryFilter(x => x.DeletedAt == null);
        modelBuilder.Entity<Visit>().HasQueryFilter(x => x.DeletedAt == null);
        modelBuilder.Entity<CommissionRecord>().HasQueryFilter(x => x.DeletedAt == null);

        base.OnModelCreating(modelBuilder);
    }
}
