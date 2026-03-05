using Shared.Domain;

namespace Shared.Infrastructure.Entities;

/// <summary>
/// Stores configurable clinic header information for document generation.
/// All printed documents (drug Rx, optical Rx, referral letters, consent forms, pharmacy labels)
/// pull clinic branding from this entity. Supports multi-branch via BranchId.
/// </summary>
public class ClinicSettings : AggregateRoot
{
    public string ClinicName { get; private set; } = string.Empty;
    public string? ClinicNameVi { get; private set; }
    public string Address { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? Fax { get; private set; }
    public string? LicenseNumber { get; private set; }
    public string? Tagline { get; private set; }
    public string? LogoBlobUrl { get; private set; }
    public string? Email { get; private set; }
    public string? Website { get; private set; }

    /// <summary>
    /// Private constructor for EF Core materialization.
    /// </summary>
    private ClinicSettings() { }

    /// <summary>
    /// Factory method for creating a new ClinicSettings entity with all required fields.
    /// </summary>
    public static ClinicSettings Create(
        BranchId branchId,
        string clinicName,
        string address,
        string? clinicNameVi = null,
        string? phone = null,
        string? fax = null,
        string? licenseNumber = null,
        string? tagline = null,
        string? logoBlobUrl = null,
        string? email = null,
        string? website = null)
    {
        var settings = new ClinicSettings
        {
            ClinicName = clinicName,
            Address = address,
            ClinicNameVi = clinicNameVi,
            Phone = phone,
            Fax = fax,
            LicenseNumber = licenseNumber,
            Tagline = tagline,
            LogoBlobUrl = logoBlobUrl,
            Email = email,
            Website = website
        };

        settings.SetBranchId(branchId);

        return settings;
    }

    /// <summary>
    /// Updates all editable clinic settings fields. Used by admin configuration.
    /// </summary>
    public void Update(
        string clinicName,
        string address,
        string? clinicNameVi = null,
        string? phone = null,
        string? fax = null,
        string? licenseNumber = null,
        string? tagline = null,
        string? email = null,
        string? website = null)
    {
        ClinicName = clinicName;
        Address = address;
        ClinicNameVi = clinicNameVi;
        Phone = phone;
        Fax = fax;
        LicenseNumber = licenseNumber;
        Tagline = tagline;
        Email = email;
        Website = website;

        SetUpdatedAt();
    }

    /// <summary>
    /// Updates the clinic logo URL. Separate method for logo-only updates (e.g., after blob upload).
    /// </summary>
    public void UpdateLogo(string? logoUrl)
    {
        LogoBlobUrl = logoUrl;
        SetUpdatedAt();
    }
}
