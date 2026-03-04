namespace Clinical.Contracts.Dtos;

/// <summary>
/// Command to create a new clinical visit (from check-in or walk-in).
/// </summary>
public record CreateVisitCommand(
    Guid PatientId,
    string PatientName,
    Guid DoctorId,
    string DoctorName,
    bool HasAllergies,
    Guid? AppointmentId);

/// <summary>
/// Command to update refraction data for a visit.
/// </summary>
public record UpdateRefractionCommand(
    Guid VisitId,
    int RefractionType,
    decimal? OdSph, decimal? OdCyl, decimal? OdAxis, decimal? OdAdd, decimal? OdPd,
    decimal? OsSph, decimal? OsCyl, decimal? OsAxis, decimal? OsAdd, decimal? OsPd,
    decimal? UcvaOd, decimal? UcvaOs, decimal? BcvaOd, decimal? BcvaOs,
    decimal? IopOd, decimal? IopOs, int? IopMethod,
    decimal? AxialLengthOd, decimal? AxialLengthOs);

/// <summary>
/// Command to add a diagnosis to a visit.
/// </summary>
public record AddVisitDiagnosisCommand(
    Guid VisitId,
    string Icd10Code,
    string DescriptionEn,
    string DescriptionVi,
    int Laterality,
    int Role,
    int SortOrder);

/// <summary>
/// Command to remove a diagnosis from a visit.
/// </summary>
public record RemoveVisitDiagnosisCommand(
    Guid VisitId,
    Guid DiagnosisId);

/// <summary>
/// Command to sign off a visit, making it immutable.
/// </summary>
public record SignOffVisitCommand(Guid VisitId);

/// <summary>
/// Command to amend a signed visit with mandatory reason and field-level diff.
/// </summary>
public record AmendVisitCommand(
    Guid VisitId,
    string Reason,
    string FieldChangesJson);

/// <summary>
/// Command to advance a visit to the next workflow stage.
/// </summary>
public record AdvanceWorkflowStageCommand(
    Guid VisitId,
    int NewStage);

/// <summary>
/// Command to update examination notes on a visit.
/// </summary>
public record UpdateVisitNotesCommand(
    Guid VisitId,
    string? Notes);

/// <summary>
/// Query to search ICD-10 codes by term with optional doctor favorites.
/// </summary>
public record SearchIcd10CodesQuery(
    string SearchTerm,
    Guid? DoctorId);

/// <summary>
/// Command to toggle an ICD-10 code as a per-doctor favorite.
/// </summary>
public record ToggleIcd10FavoriteCommand(
    Guid DoctorId,
    string Icd10Code);

/// <summary>
/// Query to get all active visits for the workflow dashboard.
/// </summary>
public record GetActiveVisitsQuery();

/// <summary>
/// Query to get a visit by ID with full details.
/// </summary>
public record GetVisitByIdQuery(Guid VisitId);

/// <summary>
/// Query to get a doctor's ICD-10 favorites.
/// </summary>
public record GetDoctorFavoritesQuery(Guid DoctorId);
