using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Domain;

namespace Auth.Infrastructure.Seeding;

/// <summary>
/// Hosted service that seeds initial auth data on startup.
/// Idempotent: only creates data if it doesn't already exist.
///
/// Seeds:
/// - All Permission records (PermissionModule x PermissionAction)
/// - 8 system roles with preset permission templates
/// - Root admin user with Admin role
/// - Default SystemSettings for token lifetimes
/// </summary>
public sealed class AuthDataSeeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthDataSeeder> _logger;

    public AuthDataSeeder(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<AuthDataSeeder> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        try
        {
            _logger.LogInformation("AuthDataSeeder: Starting data seeding...");

            await SeedPermissionsAsync(dbContext, cancellationToken);
            await SeedRolesAsync(dbContext, cancellationToken);
            await SeedRootAdminAsync(dbContext, scope.ServiceProvider, cancellationToken);
            await SeedSystemSettingsAsync(dbContext, cancellationToken);

            _logger.LogInformation("AuthDataSeeder: Data seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AuthDataSeeder: Error during data seeding.");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task SeedPermissionsAsync(AuthDbContext dbContext, CancellationToken ct)
    {
        if (await dbContext.Permissions.AnyAsync(ct))
        {
            _logger.LogInformation("AuthDataSeeder: Permissions already exist, skipping.");
            return;
        }

        var permissions = new List<Permission>();

        foreach (var module in Enum.GetValues<PermissionModule>())
        {
            foreach (var action in Enum.GetValues<PermissionAction>())
            {
                permissions.Add(new Permission(
                    module,
                    action,
                    $"{action} access to {module} module"));
            }
        }

        dbContext.Permissions.AddRange(permissions);
        await dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("AuthDataSeeder: Seeded {Count} permissions.", permissions.Count);
    }

    private async Task SeedRolesAsync(AuthDbContext dbContext, CancellationToken ct)
    {
        if (await dbContext.Roles.AnyAsync(ct))
        {
            _logger.LogInformation("AuthDataSeeder: Roles already exist, skipping.");
            return;
        }

        var allPermissions = await dbContext.Permissions.ToListAsync(ct);
        var branchId = new BranchId(Guid.Parse("00000000-0000-0000-0000-000000000001"));

        Permission GetPermission(PermissionModule module, PermissionAction action) =>
            allPermissions.First(p => p.Module == module && p.Action == action);

        List<Permission> GetModulePermissions(PermissionModule module, params PermissionAction[] actions) =>
            actions.Select(a => GetPermission(module, a)).ToList();

        // 1. Admin -- ALL permissions
        var admin = new Role("Admin", "Full system administrator with all permissions", true, branchId);
        admin.UpdatePermissions(allPermissions);

        // 2. Doctor
        var doctor = new Role("Doctor", "Medical doctor with clinical and patient access", true, branchId);
        var doctorPerms = new List<Permission>();
        doctorPerms.AddRange(GetModulePermissions(PermissionModule.Patient, PermissionAction.View, PermissionAction.Create, PermissionAction.Update));
        doctorPerms.AddRange(GetModulePermissions(PermissionModule.Clinical, PermissionAction.View, PermissionAction.Create, PermissionAction.Update));
        doctorPerms.AddRange(GetModulePermissions(PermissionModule.Scheduling, PermissionAction.View, PermissionAction.Create, PermissionAction.Update));
        doctorPerms.AddRange(GetModulePermissions(PermissionModule.Pharmacy, PermissionAction.View));
        doctorPerms.AddRange(GetModulePermissions(PermissionModule.Optical, PermissionAction.View));
        doctorPerms.AddRange(GetModulePermissions(PermissionModule.Billing, PermissionAction.View));
        doctorPerms.AddRange(GetModulePermissions(PermissionModule.Treatment, PermissionAction.Manage));
        doctor.UpdatePermissions(doctorPerms);

        // 3. Technician
        var technician = new Role("Technician", "Clinical technician with patient and clinical access", true, branchId);
        var techPerms = new List<Permission>();
        techPerms.AddRange(GetModulePermissions(PermissionModule.Patient, PermissionAction.View, PermissionAction.Create));
        techPerms.AddRange(GetModulePermissions(PermissionModule.Clinical, PermissionAction.View, PermissionAction.Create));
        techPerms.AddRange(GetModulePermissions(PermissionModule.Scheduling, PermissionAction.View));
        technician.UpdatePermissions(techPerms);

        // 4. Nurse
        var nurse = new Role("Nurse", "Nurse with patient and clinical access", true, branchId);
        var nursePerms = new List<Permission>();
        nursePerms.AddRange(GetModulePermissions(PermissionModule.Patient, PermissionAction.View, PermissionAction.Create));
        nursePerms.AddRange(GetModulePermissions(PermissionModule.Clinical, PermissionAction.View, PermissionAction.Create));
        nursePerms.AddRange(GetModulePermissions(PermissionModule.Scheduling, PermissionAction.View));
        nurse.UpdatePermissions(nursePerms);

        // 5. Cashier
        var cashier = new Role("Cashier", "Cashier with billing and pharmacy view access", true, branchId);
        var cashierPerms = new List<Permission>();
        cashierPerms.AddRange(GetModulePermissions(PermissionModule.Patient, PermissionAction.View));
        cashierPerms.AddRange(GetModulePermissions(PermissionModule.Billing, PermissionAction.View, PermissionAction.Create, PermissionAction.Update));
        cashierPerms.AddRange(GetModulePermissions(PermissionModule.Pharmacy, PermissionAction.View));
        cashier.UpdatePermissions(cashierPerms);

        // 6. OpticalStaff
        var opticalStaff = new Role("OpticalStaff", "Optical center staff with optical and billing access", true, branchId);
        var opticalPerms = new List<Permission>();
        opticalPerms.AddRange(GetModulePermissions(PermissionModule.Patient, PermissionAction.View));
        opticalPerms.AddRange(GetModulePermissions(PermissionModule.Optical, PermissionAction.View, PermissionAction.Create, PermissionAction.Update));
        opticalPerms.AddRange(GetModulePermissions(PermissionModule.Billing, PermissionAction.View));
        opticalStaff.UpdatePermissions(opticalPerms);

        // 7. Manager — TRT-09: Manager can process treatment cancellations (requires Treatment.Manage)
        var manager = new Role("Manager", "Clinic manager with broad access except clinical deletion", true, branchId);
        var managerPerms = allPermissions
            .Where(p => !(p.Module == PermissionModule.Clinical && p.Action == PermissionAction.Delete))
            .ToList();
        manager.UpdatePermissions(managerPerms);

        // 8. Accountant
        var accountant = new Role("Accountant", "Accountant with billing and audit view/export access", true, branchId);
        var accountantPerms = new List<Permission>();
        accountantPerms.AddRange(GetModulePermissions(PermissionModule.Billing, PermissionAction.View, PermissionAction.Export));
        accountantPerms.AddRange(GetModulePermissions(PermissionModule.Audit, PermissionAction.View, PermissionAction.Export));
        accountant.UpdatePermissions(accountantPerms);

        // 9. Receptionist — Front desk staff managing patient check-in and appointments
        var receptionist = new Role("Receptionist", "Front desk receptionist managing patient check-in and appointments", true, branchId);
        var receptionistPerms = new List<Permission>();
        receptionistPerms.AddRange(GetModulePermissions(PermissionModule.Patient, PermissionAction.View, PermissionAction.Create, PermissionAction.Update));
        receptionistPerms.AddRange(GetModulePermissions(PermissionModule.Scheduling, PermissionAction.View, PermissionAction.Create, PermissionAction.Update));
        receptionistPerms.AddRange(GetModulePermissions(PermissionModule.Clinical, PermissionAction.View, PermissionAction.Create));
        receptionist.UpdatePermissions(receptionistPerms);

        dbContext.Roles.AddRange(admin, doctor, technician, nurse, cashier, opticalStaff, manager, accountant, receptionist);
        await dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("AuthDataSeeder: Seeded 9 system roles with preset permissions.");
    }

    private async Task SeedRootAdminAsync(AuthDbContext dbContext, IServiceProvider sp, CancellationToken ct)
    {
        var adminEmail = _configuration["Admin:Email"] ?? "admin@ganka28.com";
        var adminPassword = _configuration["Admin:Password"] ?? "Admin@123456";

        // Check if admin already exists (ignore query filters to include soft-deleted)
        var existingAdmin = await dbContext.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == adminEmail, ct);
        if (existingAdmin is not null)
        {
            // Ensure manager PIN is set on existing admin
            if (string.IsNullOrEmpty(existingAdmin.ManagerPinHash))
            {
                var hasher = sp.GetRequiredService<IPasswordHasher>();
                var pin = _configuration["Admin:ManagerPin"] ?? "123456";
                existingAdmin.SetManagerPinHash(hasher.HashPassword(pin));
                await dbContext.SaveChangesAsync(ct);
                _logger.LogInformation("AuthDataSeeder: Set manager PIN for existing admin user.");
            }
            return;
        }

        var passwordHasher = sp.GetRequiredService<IPasswordHasher>();
        var branchId = new BranchId(Guid.Parse("00000000-0000-0000-0000-000000000001"));

        var passwordHash = passwordHasher.HashPassword(adminPassword);
        var adminUser = User.Create(adminEmail, "System Administrator", passwordHash, branchId);

        // Set default manager PIN for admin user (default: 123456)
        var adminPin = _configuration["Admin:ManagerPin"] ?? "123456";
        var pinHash = passwordHasher.HashPassword(adminPin);
        adminUser.SetManagerPinHash(pinHash);

        var adminRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Admin", ct);
        if (adminRole is not null)
        {
            adminUser.AssignRole(adminRole);
        }

        dbContext.Users.Add(adminUser);
        await dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("AuthDataSeeder: Root admin user created with email {Email}.", adminEmail);
    }

    private async Task SeedSystemSettingsAsync(AuthDbContext dbContext, CancellationToken ct)
    {
        if (await dbContext.SystemSettings.AnyAsync(ct))
        {
            _logger.LogInformation("AuthDataSeeder: SystemSettings already exist, skipping.");
            return;
        }

        var settings = new List<SystemSetting>
        {
            new("AccessTokenLifetimeMinutes", "15", "JWT access token lifetime in minutes"),
            new("RefreshTokenLifetimeDays", "7", "Refresh token lifetime in days"),
            new("RememberMeRefreshTokenLifetimeDays", "30", "Refresh token lifetime when Remember Me is checked"),
            new("SessionTimeoutMinutes", "30", "Session timeout duration for inactivity warning")
        };

        dbContext.SystemSettings.AddRange(settings);
        await dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("AuthDataSeeder: Seeded {Count} system settings.", settings.Count);
    }
}
