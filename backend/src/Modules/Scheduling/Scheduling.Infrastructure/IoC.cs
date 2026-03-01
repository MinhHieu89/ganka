using Microsoft.Extensions.DependencyInjection;
using Scheduling.Application.Interfaces;
using Scheduling.Infrastructure.Repositories;
using Scheduling.Infrastructure.Seeding;

namespace Scheduling.Infrastructure;

/// <summary>
/// DI registration for the Scheduling Infrastructure layer.
/// Registers repositories, Unit of Work, and data seeders.
/// Note: SchedulingDbContext registration remains in Bootstrapper Program.cs because it
/// requires cross-module AuditInterceptor from Audit.Infrastructure.
/// </summary>
public static class IoC
{
    public static IServiceCollection AddSchedulingInfrastructure(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<ISelfBookingRepository, SelfBookingRepository>();
        services.AddScoped<IClinicScheduleRepository, ClinicScheduleRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Data seeding
        services.AddHostedService<AppointmentTypeSeeder>();
        services.AddHostedService<ClinicScheduleSeeder>();

        return services;
    }
}
