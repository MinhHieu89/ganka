using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinical.Infrastructure.Configurations;

public class VisitConfiguration : IEntityTypeConfiguration<Visit>
{
    public void Configure(EntityTypeBuilder<Visit> builder)
    {
        builder.ToTable("Visits", "clinical");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.PatientId).IsRequired();

        builder.Property(v => v.PatientName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(v => v.DoctorId).IsRequired();

        builder.Property(v => v.DoctorName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(v => v.CurrentStage)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(v => v.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(v => v.Source)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(v => v.Reason)
            .HasMaxLength(500);

        builder.Property(v => v.CancelledReason)
            .HasMaxLength(500);

        builder.Property(v => v.VisitDate).IsRequired();

        builder.Property(v => v.ExaminationNotes)
            .HasColumnType("nvarchar(max)");

        builder.Property(v => v.HasAllergies)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(v => v.BranchId)
            .HasConversion(
                b => b.Value,
                v => new Shared.Domain.BranchId(v));

        // Optimistic concurrency via RowVersion
        builder.Property(v => v.RowVersion)
            .IsRowVersion();

        // Navigation collections
        builder.HasMany(v => v.Refractions)
            .WithOne()
            .HasForeignKey(r => r.VisitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.Diagnoses)
            .WithOne()
            .HasForeignKey(d => d.VisitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.Amendments)
            .WithOne()
            .HasForeignKey(a => a.VisitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.DryEyeAssessments)
            .WithOne()
            .HasForeignKey(d => d.VisitId)
            .OnDelete(DeleteBehavior.Cascade);

        // Backing field access mode: tells EF Core to use private _refractions, _diagnoses,
        // _dryEyeAssessments, _amendments fields directly when materializing and tracking
        // entities, rather than trying to go through the read-only IReadOnlyCollection properties.
        // Without this, EF Core cannot persist entities added via domain methods like AddRefraction().
        builder.Navigation(v => v.Refractions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(v => v.Diagnoses)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(v => v.DryEyeAssessments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(v => v.Amendments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Drug prescriptions navigation
        builder.HasMany(v => v.DrugPrescriptions)
            .WithOne()
            .HasForeignKey(d => d.VisitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(v => v.DrugPrescriptions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Optical prescriptions navigation
        builder.HasMany(v => v.OpticalPrescriptions)
            .WithOne()
            .HasForeignKey(o => o.VisitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(v => v.OpticalPrescriptions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Imaging requests navigation
        builder.HasMany(v => v.ImagingRequests)
            .WithOne()
            .HasForeignKey(ir => ir.VisitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(v => v.ImagingRequests)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Stage skips navigation
        builder.HasMany(v => v.StageSkips)
            .WithOne()
            .HasForeignKey(s => s.VisitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(v => v.StageSkips)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Visit payments navigation
        builder.HasMany(v => v.VisitPayments)
            .WithOne()
            .HasForeignKey(vp => vp.VisitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(v => v.VisitPayments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Pharmacy dispensings navigation
        builder.HasMany(v => v.PharmacyDispensings)
            .WithOne()
            .HasForeignKey(pd => pd.VisitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(v => v.PharmacyDispensings)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Optical orders navigation
        builder.HasMany(v => v.OpticalOrders)
            .WithOne()
            .HasForeignKey(oo => oo.VisitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(v => v.OpticalOrders)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Handoff checklists navigation
        builder.HasMany(v => v.HandoffChecklists)
            .WithOne()
            .HasForeignKey(hc => hc.VisitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(v => v.HandoffChecklists)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Technician orders navigation
        builder.HasMany(v => v.TechnicianOrders)
            .WithOne()
            .HasForeignKey(to => to.VisitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(v => v.TechnicianOrders)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Workflow branching properties
        builder.Property(v => v.DrugTrackStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(v => v.GlassesTrackStatus)
            .IsRequired()
            .HasConversion<int>();

        // Receptionist workflow fields
        builder.Property(v => v.Source)
            .HasConversion<int>()
            .HasDefaultValue(VisitSource.Appointment);

        builder.Property(v => v.Reason).HasMaxLength(500);
        builder.Property(v => v.CancelledReason).HasMaxLength(500);
        builder.Property(v => v.CancelledBy);

        // Performance indexes
        builder.HasIndex(v => v.PatientId);
        builder.HasIndex(v => v.DoctorId);
        builder.HasIndex(v => new { v.CurrentStage, v.Status });
    }
}
