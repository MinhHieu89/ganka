using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scheduling.Domain.Entities;

namespace Scheduling.Infrastructure.Seeding;

/// <summary>
/// Hosted service that seeds appointment types on startup.
/// Idempotent: only creates data if it doesn't already exist.
/// Seeds 4 types: NewPatient, FollowUp, Treatment, OrthoK.
/// </summary>
public sealed class AppointmentTypeSeeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AppointmentTypeSeeder> _logger;

    // Well-known IDs for seeded appointment types
    public static readonly Guid NewPatientId = Guid.Parse("00000000-0000-0000-0000-000000000101");
    public static readonly Guid FollowUpId = Guid.Parse("00000000-0000-0000-0000-000000000102");
    public static readonly Guid TreatmentId = Guid.Parse("00000000-0000-0000-0000-000000000103");
    public static readonly Guid OrthoKId = Guid.Parse("00000000-0000-0000-0000-000000000104");

    public AppointmentTypeSeeder(IServiceProvider serviceProvider, ILogger<AppointmentTypeSeeder> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SchedulingDbContext>();

        try
        {
            var existing = await dbContext.AppointmentTypes.AnyAsync(cancellationToken);
            if (existing)
            {
                _logger.LogInformation("AppointmentTypeSeeder: Types already exist, skipping.");
                return;
            }

            _logger.LogInformation("AppointmentTypeSeeder: Seeding appointment types...");

            dbContext.AppointmentTypes.AddRange(
                new AppointmentType(NewPatientId, "New Patient", "Bệnh nhân mới", 30, "#3b82f6"),
                new AppointmentType(FollowUpId, "Follow-Up", "Tái khám", 20, "#22c55e"),
                new AppointmentType(TreatmentId, "Treatment", "Điều trị", 30, "#f97316"),
                new AppointmentType(OrthoKId, "Ortho-K", "Ortho-K", 30, "#a855f7")
            );

            await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("AppointmentTypeSeeder: Seeded 4 appointment types.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AppointmentTypeSeeder: Error during seeding.");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
