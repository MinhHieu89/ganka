namespace Clinical.Infrastructure.Documents.Shared;

/// <summary>
/// Clinic header data for all document types.
/// Populated from ClinicSettings or defaults if no settings exist.
/// </summary>
public sealed record ClinicHeaderData(
    string ClinicName,
    string? ClinicNameVi,
    string? Address,
    string? Phone,
    string? Fax,
    string? LicenseNumber,
    string? Tagline,
    byte[]? LogoBytes);

/// <summary>
/// Data record for drug prescription document generation.
/// </summary>
public sealed record DrugPrescriptionData(
    string PatientName,
    DateTime? DateOfBirth,
    string? Gender,
    string? Address,
    string? IdentityNumber,
    List<string> Diagnoses,
    List<DrugPrescriptionItemData> Items,
    string? Notes,
    string DoctorName,
    DateTime PrescribedAt,
    string? PrescriptionCode);

/// <summary>
/// Individual drug item in a prescription.
/// </summary>
public sealed record DrugPrescriptionItemData(
    int SortOrder,
    string DrugName,
    string? GenericName,
    string? Strength,
    string? Dosage,
    string? DosageOverride,
    int Quantity,
    string Unit);

/// <summary>
/// Data record for optical prescription document generation.
/// </summary>
public sealed record OpticalPrescriptionData(
    string PatientName,
    DateTime? DateOfBirth,
    string? Gender,
    // Distance Rx (OD)
    decimal? OdSph, decimal? OdCyl, int? OdAxis, decimal? OdAdd,
    // Distance Rx (OS)
    decimal? OsSph, decimal? OsCyl, int? OsAxis, decimal? OsAdd,
    // Near Rx overrides (if different from distance)
    decimal? NearOdSph, decimal? NearOdCyl, int? NearOdAxis,
    decimal? NearOsSph, decimal? NearOsCyl, int? NearOsAxis,
    // PD
    decimal? FarPd, decimal? NearPd,
    string? LensType,
    string? Notes,
    string DoctorName,
    DateTime PrescribedAt);

/// <summary>
/// Data record for referral letter document generation.
/// </summary>
public sealed record ReferralLetterData(
    string PatientName,
    DateTime? DateOfBirth,
    string? Gender,
    string? Address,
    string? IdentityNumber,
    List<string> Diagnoses,
    string? ExaminationNotes,
    string ReferralReason,
    string ReferralTo,
    string DoctorName,
    DateTime ReferralDate);

/// <summary>
/// Data record for consent form document generation.
/// </summary>
public sealed record ConsentFormData(
    string PatientName,
    DateTime? DateOfBirth,
    string? Gender,
    string? Address,
    string? IdentityNumber,
    string ProcedureType,
    List<string> Diagnoses,
    string DoctorName,
    DateTime FormDate);

/// <summary>
/// Data record for pharmacy label document generation.
/// </summary>
public sealed record PharmacyLabelData(
    string ClinicName,
    string PatientName,
    string DrugName,
    string? Strength,
    string? Dosage,
    string? DosageOverride,
    int Quantity,
    string Unit,
    DateTime DispensedDate);
