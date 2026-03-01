using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scheduling.Domain.Entities;
using Shared.Domain;

namespace Scheduling.Infrastructure.Seeding;

/// <summary>
/// Hosted service that seeds clinic operating schedule on startup.
/// Idempotent: only creates data if it doesn't already exist.
/// Schedule: Monday CLOSED, Tue-Fri 13:00-20:00, Sat-Sun 08:00-12:00.
/// </summary>
public sealed class ClinicScheduleSeeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ClinicScheduleSeeder> _logger;

    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    public ClinicScheduleSeeder(IServiceProvider serviceProvider, ILogger<ClinicScheduleSeeder> logger)
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
            var existing = await dbContext.ClinicSchedules.AnyAsync(cancellationToken);
            if (existing)
            {
                _logger.LogInformation("ClinicScheduleSeeder: Schedule already exists, skipping.");
                return;
            }

            _logger.LogInformation("ClinicScheduleSeeder: Seeding clinic schedule...");

            var afternoonOpen = new TimeSpan(13, 0, 0);
            var afternoonClose = new TimeSpan(20, 0, 0);
            var morningOpen = new TimeSpan(8, 0, 0);
            var morningClose = new TimeSpan(12, 0, 0);

            dbContext.ClinicSchedules.AddRange(
                // Monday: CLOSED
                new ClinicSchedule(DayOfWeek.Monday, false, null, null, DefaultBranchId),
                // Tue-Fri: 13:00-20:00
                new ClinicSchedule(DayOfWeek.Tuesday, true, afternoonOpen, afternoonClose, DefaultBranchId),
                new ClinicSchedule(DayOfWeek.Wednesday, true, afternoonOpen, afternoonClose, DefaultBranchId),
                new ClinicSchedule(DayOfWeek.Thursday, true, afternoonOpen, afternoonClose, DefaultBranchId),
                new ClinicSchedule(DayOfWeek.Friday, true, afternoonOpen, afternoonClose, DefaultBranchId),
                // Sat-Sun: 08:00-12:00
                new ClinicSchedule(DayOfWeek.Saturday, true, morningOpen, morningClose, DefaultBranchId),
                new ClinicSchedule(DayOfWeek.Sunday, true, morningOpen, morningClose, DefaultBranchId)
            );

            await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("ClinicScheduleSeeder: Seeded 7-day clinic schedule.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ClinicScheduleSeeder: Error during seeding.");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
