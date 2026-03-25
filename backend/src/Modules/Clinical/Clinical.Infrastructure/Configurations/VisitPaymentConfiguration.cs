using Clinical.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinical.Infrastructure.Configurations;

public class VisitPaymentConfiguration : IEntityTypeConfiguration<VisitPayment>
{
    public void Configure(EntityTypeBuilder<VisitPayment> builder)
    {
        builder.ToTable("VisitPayment", "clinical");

        builder.HasKey(vp => vp.Id);

        builder.Property(vp => vp.VisitId).IsRequired();

        builder.Property(vp => vp.PaymentKind)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(vp => vp.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(vp => vp.Method)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(vp => vp.AmountReceived)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(vp => vp.ChangeGiven)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(vp => vp.CashierId).IsRequired();

        builder.Property(vp => vp.CashierName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(vp => vp.PaidAt).IsRequired();

        builder.HasIndex(vp => vp.VisitId);
    }
}
