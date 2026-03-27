using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduling.Domain.Entities;
using Scheduling.Domain.Enums;

namespace Scheduling.Infrastructure.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.PatientId).IsRequired(false); // Nullable for guest bookings (D-11)

        builder.Property(a => a.PatientName)
            .IsRequired()
            .HasMaxLength(200);

        // Guest booking fields (D-11)
        builder.Property(a => a.GuestName).HasMaxLength(200);
        builder.Property(a => a.GuestPhone).HasMaxLength(20);
        builder.Property(a => a.GuestReason).HasMaxLength(500);
        builder.Property(a => a.Source).HasConversion<int>();
        builder.Property(a => a.CheckedInAt);
        builder.Property(a => a.NoShowAt);
        builder.Property(a => a.NoShowBy);
        builder.Property(a => a.NoShowNotes).HasMaxLength(500);
        builder.Property(a => a.CancelledBy);

        builder.Property(a => a.DoctorId).IsRequired();

        builder.Property(a => a.DoctorName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.StartTime).IsRequired();
        builder.Property(a => a.EndTime).IsRequired();
        builder.Property(a => a.AppointmentTypeId).IsRequired();

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(a => a.CancellationReason)
            .HasConversion<int?>();

        builder.Property(a => a.CancellationNote)
            .HasMaxLength(500);

        builder.Property(a => a.Notes)
            .HasMaxLength(500);

        builder.Property(a => a.BranchId)
            .HasConversion(
                b => b.Value,
                v => new Shared.Domain.BranchId(v));

        // Optimistic concurrency via RowVersion
        builder.Property(a => a.RowVersion)
            .IsRowVersion();

        // Unique filtered index to prevent double-booking at database level.
        // Only considers non-cancelled appointments for the same doctor at the same start time.
        builder.HasIndex(a => new { a.DoctorId, a.StartTime })
            .IsUnique()
            .HasFilter("[Status] NOT IN (2, 4)"); // Exclude Cancelled(2) and NoShow(4)

        // Performance indexes
        builder.HasIndex(a => a.PatientId);
        builder.HasIndex(a => a.DoctorId);
        builder.HasIndex(a => a.StartTime);
    }
}
