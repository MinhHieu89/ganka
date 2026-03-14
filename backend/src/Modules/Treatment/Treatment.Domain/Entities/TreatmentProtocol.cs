using Shared.Domain;
using Treatment.Domain.Enums;
using Treatment.Domain.Events;

namespace Treatment.Domain.Entities;

/// <summary>
/// Aggregate root for a treatment protocol template.
/// Doctors configure protocols (e.g., "Standard IPL 4-session") that serve as blueprints
/// when creating patient treatment packages — providing default session counts, pricing, and parameters.
/// </summary>
public class TreatmentProtocol : AggregateRoot, IAuditable
{
    /// <summary>Template name, e.g., "Standard IPL 4-session"</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Type of treatment this protocol applies to</summary>
    public TreatmentType TreatmentType { get; private set; }

    /// <summary>Default number of sessions in a package (1-6)</summary>
    public int DefaultSessionCount { get; private set; }

    /// <summary>Pricing model: per session or per package</summary>
    public PricingMode PricingMode { get; private set; }

    /// <summary>Default total package price in VND (used when PricingMode is PerPackage)</summary>
    public decimal DefaultPackagePrice { get; private set; }

    /// <summary>Default per-session price in VND (used when PricingMode is PerSession)</summary>
    public decimal DefaultSessionPrice { get; private set; }

    /// <summary>Minimum recommended interval in days between sessions</summary>
    public int MinIntervalDays { get; private set; }

    /// <summary>Maximum recommended interval in days between sessions</summary>
    public int MaxIntervalDays { get; private set; }

    /// <summary>
    /// JSON representation of typed treatment parameters (IplParameters / LlltParameters / LidCareParameters).
    /// Stored as JSON string for flexibility; deserialized to the appropriate value object based on TreatmentType.
    /// </summary>
    public string? DefaultParametersJson { get; private set; }

    /// <summary>
    /// Percentage deducted from package price upon patient cancellation (range 10-20, default 15).
    /// Per TRT-09 business rule.
    /// </summary>
    public decimal CancellationDeductionPercent { get; private set; }

    /// <summary>Whether this protocol template is active. False = soft-deactivated (hidden from selection).</summary>
    public bool IsActive { get; private set; }

    /// <summary>Optional description or notes about this protocol template</summary>
    public string? Description { get; private set; }

    /// <summary>Private parameterless constructor for EF Core materialization.</summary>
    private TreatmentProtocol() { }

    /// <summary>
    /// Factory method for creating a new treatment protocol template.
    /// Validates business invariants: session count 1-6, deduction percent 10-20, prices non-negative.
    /// </summary>
    /// <param name="name">Template name (required, max 200 chars).</param>
    /// <param name="treatmentType">Type of treatment.</param>
    /// <param name="defaultSessionCount">Number of sessions per package (1-6).</param>
    /// <param name="pricingMode">Pricing model.</param>
    /// <param name="defaultPackagePrice">Package price in VND (>= 0).</param>
    /// <param name="defaultSessionPrice">Per-session price in VND (>= 0).</param>
    /// <param name="minIntervalDays">Minimum days between sessions.</param>
    /// <param name="maxIntervalDays">Maximum days between sessions.</param>
    /// <param name="defaultParametersJson">JSON of treatment-type-specific parameters (nullable).</param>
    /// <param name="cancellationDeductionPercent">Cancellation fee percentage (10-20).</param>
    /// <param name="description">Optional description.</param>
    /// <param name="branchId">Branch this protocol belongs to (multi-tenant isolation).</param>
    public static TreatmentProtocol Create(
        string name,
        TreatmentType treatmentType,
        int defaultSessionCount,
        PricingMode pricingMode,
        decimal defaultPackagePrice,
        decimal defaultSessionPrice,
        int minIntervalDays,
        int maxIntervalDays,
        string? defaultParametersJson,
        decimal cancellationDeductionPercent,
        string? description,
        BranchId branchId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Protocol name is required.", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("Protocol name must not exceed 200 characters.", nameof(name));

        if (defaultSessionCount < 1 || defaultSessionCount > 6)
            throw new ArgumentOutOfRangeException(
                nameof(defaultSessionCount),
                "Default session count must be between 1 and 6.");

        if (cancellationDeductionPercent < 10 || cancellationDeductionPercent > 20)
            throw new ArgumentOutOfRangeException(
                nameof(cancellationDeductionPercent),
                "Cancellation deduction percent must be between 10 and 20.");

        if (defaultPackagePrice < 0)
            throw new ArgumentOutOfRangeException(
                nameof(defaultPackagePrice),
                "Default package price must be non-negative.");

        if (defaultSessionPrice < 0)
            throw new ArgumentOutOfRangeException(
                nameof(defaultSessionPrice),
                "Default session price must be non-negative.");

        if (minIntervalDays < 0)
            throw new ArgumentOutOfRangeException(
                nameof(minIntervalDays),
                "Minimum interval days must be non-negative.");

        if (maxIntervalDays < minIntervalDays)
            throw new ArgumentOutOfRangeException(
                nameof(maxIntervalDays),
                "Maximum interval days must be greater than or equal to minimum interval days.");

        var protocol = new TreatmentProtocol
        {
            Name = name,
            TreatmentType = treatmentType,
            DefaultSessionCount = defaultSessionCount,
            PricingMode = pricingMode,
            DefaultPackagePrice = defaultPackagePrice,
            DefaultSessionPrice = defaultSessionPrice,
            MinIntervalDays = minIntervalDays,
            MaxIntervalDays = maxIntervalDays,
            DefaultParametersJson = defaultParametersJson,
            CancellationDeductionPercent = cancellationDeductionPercent,
            Description = description,
            IsActive = true
        };

        protocol.SetBranchId(branchId);
        return protocol;
    }

    /// <summary>
    /// Updates all editable attributes of the protocol template.
    /// Re-validates business invariants on update.
    /// </summary>
    public void Update(
        string name,
        TreatmentType treatmentType,
        int defaultSessionCount,
        PricingMode pricingMode,
        decimal defaultPackagePrice,
        decimal defaultSessionPrice,
        int minIntervalDays,
        int maxIntervalDays,
        string? defaultParametersJson,
        decimal cancellationDeductionPercent,
        string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Protocol name is required.", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("Protocol name must not exceed 200 characters.", nameof(name));

        if (defaultSessionCount < 1 || defaultSessionCount > 6)
            throw new ArgumentOutOfRangeException(
                nameof(defaultSessionCount),
                "Default session count must be between 1 and 6.");

        if (cancellationDeductionPercent < 10 || cancellationDeductionPercent > 20)
            throw new ArgumentOutOfRangeException(
                nameof(cancellationDeductionPercent),
                "Cancellation deduction percent must be between 10 and 20.");

        if (defaultPackagePrice < 0)
            throw new ArgumentOutOfRangeException(
                nameof(defaultPackagePrice),
                "Default package price must be non-negative.");

        if (defaultSessionPrice < 0)
            throw new ArgumentOutOfRangeException(
                nameof(defaultSessionPrice),
                "Default session price must be non-negative.");

        if (minIntervalDays < 0)
            throw new ArgumentOutOfRangeException(
                nameof(minIntervalDays),
                "Minimum interval days must be non-negative.");

        if (maxIntervalDays < minIntervalDays)
            throw new ArgumentOutOfRangeException(
                nameof(maxIntervalDays),
                "Maximum interval days must be greater than or equal to minimum interval days.");

        Name = name;
        TreatmentType = treatmentType;
        DefaultSessionCount = defaultSessionCount;
        PricingMode = pricingMode;
        DefaultPackagePrice = defaultPackagePrice;
        DefaultSessionPrice = defaultSessionPrice;
        MinIntervalDays = minIntervalDays;
        MaxIntervalDays = maxIntervalDays;
        DefaultParametersJson = defaultParametersJson;
        CancellationDeductionPercent = cancellationDeductionPercent;
        Description = description;

        SetUpdatedAt();

        AddDomainEvent(new ProtocolUpdatedEvent(
            ProtocolId: Id,
            Name: Name,
            TreatmentType: TreatmentType));
    }

    /// <summary>
    /// Soft-deactivates the protocol template, hiding it from protocol selection.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();

        AddDomainEvent(new ProtocolDeactivatedEvent(ProtocolId: Id));
    }

    /// <summary>
    /// Re-activates a previously deactivated protocol template.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();

        AddDomainEvent(new ProtocolActivatedEvent(ProtocolId: Id));
    }
}
