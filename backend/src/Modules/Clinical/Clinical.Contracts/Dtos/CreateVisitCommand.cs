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
/// Command to set a diagnosis as the primary diagnosis on a visit.
/// </summary>
public record SetPrimaryDiagnosisCommand(
    Guid VisitId,
    Guid DiagnosisId);

/// <summary>
/// Command to sign off a visit, making it immutable.
/// Optionally accepts FieldChangesJson for re-sign after amendment,
/// containing the actual before/after diff computed at re-sign time.
/// </summary>
public record SignOffVisitCommand(Guid VisitId, string? FieldChangesJson = null);

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

/// <summary>
/// Command to add/replace optical prescription (glasses Rx) on a visit.
/// Only one optical Rx per visit -- SetOpticalPrescription clears existing.
/// </summary>
public record AddOpticalPrescriptionCommand(
    Guid VisitId,
    decimal? OdSph, decimal? OdCyl, int? OdAxis, decimal? OdAdd,
    decimal? OsSph, decimal? OsCyl, int? OsAxis, decimal? OsAdd,
    decimal? FarPd, decimal? NearPd,
    decimal? NearOdSph, decimal? NearOdCyl, int? NearOdAxis,
    decimal? NearOsSph, decimal? NearOsCyl, int? NearOsAxis,
    int LensType, string? Notes);

/// <summary>
/// Command to update an existing optical prescription on a visit.
/// </summary>
public record UpdateOpticalPrescriptionCommand(
    Guid VisitId,
    Guid PrescriptionId,
    decimal? OdSph, decimal? OdCyl, int? OdAxis, decimal? OdAdd,
    decimal? OsSph, decimal? OsCyl, int? OsAxis, decimal? OsAdd,
    decimal? FarPd, decimal? NearPd,
    decimal? NearOdSph, decimal? NearOdCyl, int? NearOdAxis,
    decimal? NearOsSph, decimal? NearOsCyl, int? NearOsAxis,
    int LensType, string? Notes);

/// <summary>
/// Input for a single drug line in a prescription.
/// </summary>
public record PrescriptionItemInput(
    Guid? DrugCatalogItemId,
    string DrugName,
    string? GenericName,
    string? Strength,
    int Form,
    int Route,
    string? Dosage,
    string? DosageOverride,
    int Quantity,
    string Unit,
    string? Frequency,
    int? DurationDays,
    bool HasAllergyWarning);

/// <summary>
/// Command to add a drug prescription with items to a visit.
/// </summary>
public record AddDrugPrescriptionCommand(
    Guid VisitId,
    string? Notes,
    List<PrescriptionItemInput> Items);

/// <summary>
/// Command to update notes on an existing drug prescription.
/// </summary>
public record UpdateDrugPrescriptionCommand(
    Guid VisitId,
    Guid PrescriptionId,
    string? Notes);

/// <summary>
/// Command to remove a drug prescription from a visit.
/// </summary>
public record RemoveDrugPrescriptionCommand(
    Guid VisitId,
    Guid PrescriptionId);

/// <summary>
/// Query to check if a drug triggers any patient allergy warnings.
/// Returns matching allergies for cross-reference display.
/// </summary>
public record CheckDrugAllergyQuery(
    Guid PatientId,
    string DrugName,
    string? GenericName);

/// <summary>
/// Cross-module query to retrieve the count of active visits.
/// Handled by Clinical.Application.
/// </summary>
public record GetActiveVisitCountQuery();

/// <summary>
/// Command to reverse a visit to an earlier workflow stage with a mandatory reason (D-07/D-09).
/// </summary>
public record ReverseWorkflowStageCommand(Guid VisitId, int TargetStage, string Reason);

/// <summary>
/// Query to get patient visit history ordered by date descending (D-13/D-15).
/// </summary>
public record GetPatientVisitHistoryQuery(Guid PatientId);

// ===================== Workflow Action Commands =====================

/// <summary>
/// Command to skip refraction (RefractionVA) stage for a visit.
/// </summary>
public record SkipRefractionCommand(
    Guid VisitId,
    int Reason,
    string? FreeTextNote);

/// <summary>
/// Command to undo a refraction skip.
/// </summary>
public record UndoRefractionSkipCommand(Guid VisitId);

/// <summary>
/// Command to request imaging during DoctorExam stage.
/// </summary>
public record RequestImagingCommand(
    Guid VisitId,
    string? Note,
    List<string> Services);

/// <summary>
/// Command to complete all imaging services for a visit.
/// </summary>
public record CompleteImagingServicesCommand(Guid VisitId);

/// <summary>
/// Command to confirm visit payment at Cashier.
/// </summary>
public record ConfirmVisitPaymentCommand(
    Guid VisitId,
    decimal Amount,
    int PaymentMethod,
    decimal AmountReceived,
    bool SplitGlasses);

/// <summary>
/// Command to dispense pharmacy drugs for a visit.
/// </summary>
public record DispensePharmacyCommand(
    Guid VisitId,
    List<DispenseItemInput> DispensedItems,
    string? Note);

/// <summary>
/// Input for a single dispensed drug item.
/// </summary>
public record DispenseItemInput(
    string DrugName,
    int Quantity,
    string Instruction);

/// <summary>
/// Command to confirm optical order at OpticalCenter.
/// </summary>
public record ConfirmOpticalOrderCommand(
    Guid VisitId,
    string LensType,
    string FrameCode,
    decimal LensCost,
    decimal FrameCost,
    decimal TotalPrice);

/// <summary>
/// Command to complete optical lab processing.
/// </summary>
public record CompleteOpticalLabCommand(
    Guid VisitId,
    List<string> QualityChecklist);

/// <summary>
/// Command to complete handoff (glasses return to patient).
/// </summary>
public record CompleteHandoffCommand(
    Guid VisitId,
    bool PrescriptionVerified,
    bool FrameCorrect,
    bool PatientConfirmedFit);
