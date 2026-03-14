using Shared.Domain;

namespace Shared.Application.Interfaces;

/// <summary>
/// Service interface for retrieving and managing clinic settings.
/// Used by document generators to fetch clinic header data (logo, name, address, etc.).
/// </summary>
public interface IClinicSettingsService
{
    /// <summary>
    /// Gets the clinic settings for the current branch.
    /// Returns null if no settings configured yet.
    /// </summary>
    Task<ClinicSettingsDto?> GetCurrentAsync(CancellationToken ct);

    /// <summary>
    /// Creates or updates clinic settings for the current branch.
    /// Uses upsert pattern -- if settings exist, updates; otherwise creates.
    /// </summary>
    Task<Result<Guid>> CreateOrUpdateAsync(UpdateClinicSettingsCommand command, CancellationToken ct);

    /// <summary>
    /// Updates only the logo blob URL for the current branch's clinic settings.
    /// Creates default settings if none exist yet.
    /// </summary>
    Task UpdateLogoUrlAsync(string logoUrl, CancellationToken ct);
}

/// <summary>
/// DTO for returning clinic settings to consumers (document generators, admin UI).
/// </summary>
public sealed record ClinicSettingsDto(
    Guid Id,
    string ClinicName,
    string? ClinicNameVi,
    string Address,
    string? Phone,
    string? Fax,
    string? LicenseNumber,
    string? Tagline,
    string? LogoBlobUrl,
    string? Email,
    string? Website);

/// <summary>
/// Command for creating or updating clinic settings via admin configuration.
/// </summary>
public sealed record UpdateClinicSettingsCommand(
    string ClinicName,
    string Address,
    string? ClinicNameVi = null,
    string? Phone = null,
    string? Fax = null,
    string? LicenseNumber = null,
    string? Tagline = null,
    string? Email = null,
    string? Website = null);
