using Microsoft.EntityFrameworkCore;
using PropertyService.Domain.Entities;


namespace PropertyService.Infrastructure.Persistence;

public class PropertyDbContext : DbContext
{
    public PropertyDbContext(DbContextOptions<PropertyDbContext> options) : base(options) { }

    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<UnitOccupancy> UnitOccupancies => Set<UnitOccupancy>();
    public DbSet<PropertyUtility> PropertyUtilities => Set<PropertyUtility>();
    public DbSet<PropertyMedia> PropertyMedia => Set<PropertyMedia>();
    public DbSet<ConditionLog> ConditionLogs => Set<ConditionLog>();
    public DbSet<ConditionLogMedia> ConditionLogMedia => Set<ConditionLogMedia>();
    public DbSet<PropertyTimelineEvent> PropertyTimelineEvents => Set<PropertyTimelineEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Property>().HasQueryFilter(x => x.DeletedAt == null);
        modelBuilder.Entity<Unit>().HasQueryFilter(x => x.DeletedAt == null);

        modelBuilder.Entity<Property>()
            .HasMany(p => p.Units)
            .WithOne(u => u.Property)
            .HasForeignKey(u => u.PropertyId);

        modelBuilder.Entity<Unit>()
            .HasMany(u => u.Occupancies)
            .WithOne(o => o.Unit)
            .HasForeignKey(o => o.UnitId);

        modelBuilder.Entity<Unit>()
            .HasMany(u => u.ConditionLogs)
            .WithOne(c => c.Unit)
            .HasForeignKey(c => c.UnitId);

        modelBuilder.Entity<ConditionLog>()
            .HasMany(c => c.Media)
            .WithOne(m => m.ConditionLog)
            .HasForeignKey(m => m.ConditionLogId);

        modelBuilder.Entity<Property>()
            .HasMany(p => p.Timeline)
            .WithOne(t => t.Property)
            .HasForeignKey(t => t.PropertyId);

        modelBuilder.Entity<Unit>()
    .HasIndex(u => new { u.PropertyId, u.UnitNumber })
    .IsUnique();

        modelBuilder.Entity<Property>()
    .HasIndex(p => new { p.PropertyCode })
    .IsUnique();

        modelBuilder.Entity<PropertyMedia>().HasQueryFilter(x => x.DeletedAt == null);
        modelBuilder.Entity<PropertyUtility>().HasQueryFilter(x => x.DeletedAt == null);
        modelBuilder.Entity<PropertyTimelineEvent>().HasQueryFilter(x => x.DeletedAt == null);
        modelBuilder.Entity<UnitOccupancy>().HasQueryFilter(x => x.DeletedAt == null);
        modelBuilder.Entity<ConditionLog>().HasQueryFilter(x => x.DeletedAt == null);
        modelBuilder.Entity<ConditionLogMedia>().HasQueryFilter(x => x.DeletedAt == null);



        base.OnModelCreating(modelBuilder);
    }
}
