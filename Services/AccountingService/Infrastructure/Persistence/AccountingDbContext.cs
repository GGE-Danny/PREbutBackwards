using AccountingService.Domain.Entities;
using AccountingService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace AccountingService.Infrastructure.Persistence;

public class AccountingDbContext : DbContext
{
    public AccountingDbContext(DbContextOptions<AccountingDbContext> options) : base(options) { }

    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<OwnerDisbursement> OwnerDisbursements => Set<OwnerDisbursement>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<UnitRate> UnitRates => Set<UnitRate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountingDbContext).Assembly);

        // Global soft-delete filters
        modelBuilder.Entity<Invoice>().HasQueryFilter(x => x.DeletedAt == null);
        modelBuilder.Entity<Payment>().HasQueryFilter(x => x.DeletedAt == null);
        modelBuilder.Entity<Expense>().HasQueryFilter(x => x.DeletedAt == null);
        modelBuilder.Entity<OwnerDisbursement>().HasQueryFilter(x => x.DeletedAt == null);
        modelBuilder.Entity<LedgerEntry>().HasQueryFilter(x => x.DeletedAt == null);
        modelBuilder.Entity<UnitRate>().HasQueryFilter(x => x.DeletedAt == null);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Note: Ledger entries for Expenses, Payments, and Disbursements are now
        // created explicitly in controllers to avoid duplicates and provide better
        // control over audit notes (including actor info).
        // This hook is kept minimal - only base SaveChanges.

        return await base.SaveChangesAsync(cancellationToken);
    }
}
