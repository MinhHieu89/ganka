namespace Clinical.Contracts.Dtos;

/// <summary>
/// DTO for drug prescription data transfer.
/// Contains the prescription header and its line items.
/// </summary>
public sealed record DrugPrescriptionDto(
    Guid Id, Guid VisitId, string? Notes, string? PrescriptionCode,
    DateTime PrescribedAt, List<PrescriptionItemDto> Items);

/// <summary>
/// DTO for an individual drug line in a prescription.
/// Form and Route are int values matching Pharmacy.Domain enums.
/// </summary>
public sealed record PrescriptionItemDto(
    Guid Id, Guid? DrugCatalogItemId, string DrugName, string? GenericName,
    string? Strength, int Form, int Route, string? Dosage, string? DosageOverride,
    int Quantity, string Unit, string? Frequency, int? DurationDays,
    bool IsOffCatalog, bool HasAllergyWarning, int SortOrder);

/// <summary>
/// DTO for optical prescription (glasses Rx) data transfer.
/// Contains OD/OS distance and near refraction parameters, PD, and lens type.
/// </summary>
public sealed record OpticalPrescriptionDto(
    Guid Id, Guid VisitId,
    decimal? OdSph, decimal? OdCyl, int? OdAxis, decimal? OdAdd,
    decimal? OsSph, decimal? OsCyl, int? OsAxis, decimal? OsAdd,
    decimal? FarPd, decimal? NearPd,
    decimal? NearOdSph, decimal? NearOdCyl, int? NearOdAxis,
    decimal? NearOsSph, decimal? NearOsCyl, int? NearOsAxis,
    int LensType, string? Notes, DateTime PrescribedAt);
