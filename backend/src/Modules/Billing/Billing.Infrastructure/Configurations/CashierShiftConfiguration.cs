using Billing.Domain.Entities;
using Billing.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for the CashierShift aggregate.
/// Enforces one open shift per branch via filtered unique index.
/// All monetary fields use precision(18, 0) for VND.
/// </summary>
public class CashierShiftConfiguration : IEntityTypeConfiguration<CashierShift>
{
    public void Configure(EntityTypeBuilder<CashierShift> builder)
    {
        builder.ToTable("CashierShifts");

        builder.HasKey(cs => cs.Id);

        builder.Property(cs => cs.CashierName)
            .IsRequired()
            .HasMaxLength(200);

        // All decimal fields use precision(18, 0) for VND (no decimal places)
        builder.Property(cs => cs.OpeningBalance)
            .HasPrecision(18, 0);

        builder.Property(cs => cs.CashReceived)
            .HasPrecision(18, 0);

        builder.Property(cs => cs.CashRefunds)
            .HasPrecision(18, 0);

        builder.Property(cs => cs.ActualCashCount)
            .HasPrecision(18, 0);

        builder.Property(cs => cs.Discrepancy)
            .HasPrecision(18, 0);

        builder.Property(cs => cs.TotalRevenue)
            .HasPrecision(18, 0);

        builder.Property(cs => cs.ManagerNote)
            .HasMaxLength(1000);

        // Status stored as int
        builder.Property(cs => cs.Status)
            .HasConversion<int>();

        // BranchId value object conversion
        builder.Property(cs => cs.BranchId)
            .HasConversion(
                b => b.Value,
                v => new Shared.Domain.BranchId(v));

        // Ignore computed property ExpectedCashAmount (not stored in DB)
        builder.Ignore(cs => cs.ExpectedCashAmount);

        // Filtered unique index: enforce max one open shift per branch
        // WHERE Status = 0 (Open)
        builder.HasIndex(cs => new { cs.BranchId, cs.Status })
            .HasFilter("[Status] = 0")
            .IsUnique();

        // Performance index on CashierId
        builder.HasIndex(cs => cs.CashierId);

        // Soft-delete query filter
        builder.HasQueryFilter(cs => !cs.IsDeleted);
    }
}
