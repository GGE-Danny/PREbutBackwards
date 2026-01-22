using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using TenantService.Domain.Entities;

namespace TenantService.Infrastructure.Persistence;

public class TenantDbContext : DbContext
{
    public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options) { }

    public DbSet<TenantProfile> TenantProfiles => Set<TenantProfile>();
    public DbSet<TenantDocument> TenantDocuments => Set<TenantDocument>();
    public DbSet<TenantResidencyHistory> TenantResidencies => Set<TenantResidencyHistory>();
    public DbSet<TenantMeterAssociation> TenantMeterAssociations => Set<TenantMeterAssociation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TenantProfile>().HasQueryFilter(x => x.DeletedAt == null);
        modelBuilder.Entity<TenantDocument>().HasQueryFilter(x => x.DeletedAt == null);
        modelBuilder.Entity<TenantResidencyHistory>().HasQueryFilter(x => x.DeletedAt == null);
        modelBuilder.Entity<TenantMeterAssociation>().HasQueryFilter(x => x.DeletedAt == null);

        modelBuilder.Entity<TenantProfile>()
            .HasIndex(x => x.TenantUserId)
            .IsUnique();

        modelBuilder.Entity<TenantDocument>()
            .HasOne(d => d.TenantProfile)
            .WithMany(p => p.Documents)
            .HasForeignKey(d => d.TenantProfileId);

        base.OnModelCreating(modelBuilder);
    }
}
