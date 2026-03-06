using Billing.Domain.Entities;
using Billing.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Infrastructure.Configurations;

/// <summary>
/// EF Core configuration for CashierShift aggregate root.
/// Enforces one open shift per branch via filtered unique index.
/// All monetary fields use precision(18, 0) for VND.
/// </summary>
public sealed class CashierShiftConfiguration : IEntityTypeConfiguration<CashierShift>
{
    public void Configure(EntityTypeBuilder<CashierShift> builder)
    {
        builder.ToTable("CashierShifts");

        builder.Property(cs => cs.CashierName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(cs => cs.OpeningBalance).HasPrecision(18, 0);
        builder.Property(cs => cs.CashReceived).HasPrecision(18, 0);
        builder.Property(cs => cs.CashRefunds).HasPrecision(18, 0);
        builder.Property(cs => cs.ActualCashCount).HasPrecision(18, 0);
        builder.Property(cs => cs.Discrepancy).HasPrecision(18, 0);
        builder.Property(cs => cs.TotalRevenue).HasPrecision(18, 0);

        builder.Property(cs => cs.ManagerNote).HasMaxLength(1000);

        builder.Property(cs => cs.Status).HasConversion<int>();

        builder.Property(cs => cs.BranchId)
            .HasConversion(
                b => b.Value,
                v => new Shared.Domain.BranchId(v));

        builder.Ignore(cs => cs.ExpectedCashAmount);

        // Filtered unique index: max one open shift per branch
        builder.HasIndex(cs => new { cs.BranchId, cs.Status })
            .HasFilter($"[Status] = {(int)ShiftStatus.Open}")
            .IsUnique();

        builder.HasIndex(cs => cs.CashierId);

        builder.HasQueryFilter(cs => !cs.IsDeleted);
    }
}
