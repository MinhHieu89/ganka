namespace Scheduling.Contracts.Dtos;

/// <summary>
/// DTO for appointment data used in calendar display and lists.
/// Constructor enforces DateTimeKind.Utc on StartTime/EndTime per global UTC convention (D-03).
/// EF Core handles UTC at the DB read level (ModelBuilderExtensions.ApplySharedConventions),
/// this constructor handles it at the DTO mapping level for all callers.
/// </summary>
public record AppointmentDto
{
    public Guid Id { get; init; }
    public Guid PatientId { get; init; }
    public string PatientName { get; init; }
    public Guid DoctorId { get; init; }
    public string DoctorName { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public Guid AppointmentTypeId { get; init; }
    public string AppointmentTypeName { get; init; }
    public string AppointmentTypeNameVi { get; init; }
    public int Status { get; init; }
    public string Color { get; init; }
    public string? Notes { get; init; }

    public AppointmentDto(
        Guid Id, Guid PatientId, string PatientName,
        Guid DoctorId, string DoctorName,
        DateTime StartTime, DateTime EndTime,
        Guid AppointmentTypeId,
        string AppointmentTypeName, string AppointmentTypeNameVi,
        int Status, string Color, string? Notes)
    {
        this.Id = Id;
        this.PatientId = PatientId;
        this.PatientName = PatientName;
        this.DoctorId = DoctorId;
        this.DoctorName = DoctorName;
        this.StartTime = DateTime.SpecifyKind(StartTime, DateTimeKind.Utc);
        this.EndTime = DateTime.SpecifyKind(EndTime, DateTimeKind.Utc);
        this.AppointmentTypeId = AppointmentTypeId;
        this.AppointmentTypeName = AppointmentTypeName;
        this.AppointmentTypeNameVi = AppointmentTypeNameVi;
        this.Status = Status;
        this.Color = Color;
        this.Notes = Notes;
    }
}
