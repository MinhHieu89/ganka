using Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;

namespace Billing.Infrastructure;

/// <summary>
/// EF Core DbContext for the Billing module.
/// Uses schema-per-module isolation with the "billing" schema.
/// </summary>
public class BillingDbContext : DbContext
{
    public BillingDbContext(DbContextOptions<BillingDbContext> options) : base(options)
    {
    }

    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLineItem> InvoiceLineItems => Set<InvoiceLineItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Discount> Discounts => Set<Discount>();
    public DbSet<Refund> Refunds => Set<Refund>();
    public DbSet<CashierShift> CashierShifts => Set<CashierShift>();
    public DbSet<ShiftTemplate> ShiftTemplates => Set<ShiftTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("billing");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BillingDbContext).Assembly);

        modelBuilder.ApplySharedConventions();

        base.OnModelCreating(modelBuilder);
    }
}
