using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Application;
using Shared.Application.Interfaces;
using Shared.Domain;
using Shared.Infrastructure.Entities;

namespace Shared.Infrastructure.Services;

/// <summary>
/// Implementation of IClinicSettingsService that loads clinic settings from the database.
/// Queries by current branch for multi-tenant support.
/// </summary>
public sealed class ClinicSettingsService : IClinicSettingsService
{
    private readonly ReferenceDbContext _dbContext;
    private readonly IBranchContext _branchContext;
    private readonly ILogger<ClinicSettingsService> _logger;

    public ClinicSettingsService(
        ReferenceDbContext dbContext,
        IBranchContext branchContext,
        ILogger<ClinicSettingsService> logger)
    {
        _dbContext = dbContext;
        _branchContext = branchContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ClinicSettingsDto?> GetCurrentAsync(CancellationToken ct)
    {
        var branchId = new BranchId(_branchContext.CurrentBranchId);

        var settings = await _dbContext.ClinicSettings
            .AsNoTracking()
            .Where(cs => cs.BranchId == branchId && !cs.IsDeleted)
            .Select(cs => new ClinicSettingsDto(
                cs.Id,
                cs.ClinicName,
                cs.ClinicNameVi,
                cs.Address,
                cs.Phone,
                cs.Fax,
                cs.LicenseNumber,
                cs.Tagline,
                cs.LogoBlobUrl,
                cs.Email,
                cs.Website))
            .FirstOrDefaultAsync(ct);

        return settings;
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> CreateOrUpdateAsync(UpdateClinicSettingsCommand command, CancellationToken ct)
    {
        var branchId = new BranchId(_branchContext.CurrentBranchId);

        var existing = await _dbContext.ClinicSettings
            .Where(cs => cs.BranchId == branchId && !cs.IsDeleted)
            .FirstOrDefaultAsync(ct);

        if (existing is not null)
        {
            existing.Update(
                command.ClinicName,
                command.Address,
                command.ClinicNameVi,
                command.Phone,
                command.Fax,
                command.LicenseNumber,
                command.Tagline,
                command.Email,
                command.Website);

            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("Updated clinic settings {SettingsId} for branch {BranchId}",
                existing.Id, branchId);

            return existing.Id;
        }

        var settings = ClinicSettings.Create(
            branchId,
            command.ClinicName,
            command.Address,
            command.ClinicNameVi,
            command.Phone,
            command.Fax,
            command.LicenseNumber,
            command.Tagline,
            email: command.Email,
            website: command.Website);

        _dbContext.ClinicSettings.Add(settings);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Created clinic settings {SettingsId} for branch {BranchId}",
            settings.Id, branchId);

        return settings.Id;
    }
}
