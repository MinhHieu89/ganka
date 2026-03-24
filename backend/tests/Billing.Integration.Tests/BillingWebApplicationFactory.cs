using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Auth.Infrastructure;
using Auth.Infrastructure.Services;
using Auth.Domain.Entities;
using Auth.Domain.Enums;
using Billing.Domain.Entities;
using Billing.Domain.Enums;
using Billing.Infrastructure;
using Shared.Domain;
using Shared.Infrastructure.Interceptors;

namespace Billing.Integration.Tests;

/// <summary>
/// Custom WebApplicationFactory for Billing integration tests.
/// Uses a dedicated test database (LocalDB) with a unique name per test run.
/// Seeds a test user, patient record, and a finalized invoice with line items and payment
/// for print endpoint testing.
/// </summary>
public class BillingWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string TestUserEmail = "billingtest@ganka28.com";
    public const string TestUserPassword = "BillingTest@123";
    public const string TestUserFullName = "Billing Test User";

    private readonly string _testDbName = $"Ganka28_BillingIntTest_{Guid.NewGuid():N}";

    /// <summary>The seeded invoice ID for use in print endpoint tests.</summary>
    public Guid SeededInvoiceId { get; private set; }

    private string TestConnectionString =>
        $"Server=(localdb)\\mssqllocaldb;Database={_testDbName};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        var connectionString = TestConnectionString;

        builder.ConfigureTestServices(services =>
        {
            // Remove AuthDataSeeder hosted service to avoid WolverineHasNotStartedException
            var seederDescriptor = services.SingleOrDefault(
                d => d.ImplementationType == typeof(Auth.Infrastructure.Seeding.AuthDataSeeder));
            if (seederDescriptor != null)
                services.Remove(seederDescriptor);

            // Remove DomainEventDispatcherInterceptor -- Wolverine not reliably started during test init
            var interceptorDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DomainEventDispatcherInterceptor));
            if (interceptorDescriptor != null)
                services.Remove(interceptorDescriptor);

            // Remove all DbContext registrations
            RemoveDbContextServices<AuthDbContext>(services);
            RemoveDbContextServices<Audit.Infrastructure.AuditDbContext>(services);
            RemoveDbContextServices<Shared.Infrastructure.ReferenceDbContext>(services);
            RemoveDbContextServices<Patient.Infrastructure.PatientDbContext>(services);
            RemoveDbContextServices<Clinical.Infrastructure.ClinicalDbContext>(services);
            RemoveDbContextServices<Scheduling.Infrastructure.SchedulingDbContext>(services);
            RemoveDbContextServices<Pharmacy.Infrastructure.PharmacyDbContext>(services);
            RemoveDbContextServices<Optical.Infrastructure.OpticalDbContext>(services);
            RemoveDbContextServices<BillingDbContext>(services);
            RemoveDbContextServices<Treatment.Infrastructure.TreatmentDbContext>(services);

            // Re-register all DbContexts with test database (no interceptors)
            services.AddDbContext<AuthDbContext>(options =>
                options.UseSqlServer(connectionString));
            services.AddDbContext<Audit.Infrastructure.AuditDbContext>(options =>
                options.UseSqlServer(connectionString));
            services.AddDbContext<Shared.Infrastructure.ReferenceDbContext>(options =>
                options.UseSqlServer(connectionString));
            services.AddDbContext<Patient.Infrastructure.PatientDbContext>(options =>
                options.UseSqlServer(connectionString));
            services.AddDbContext<Clinical.Infrastructure.ClinicalDbContext>(options =>
                options.UseSqlServer(connectionString));
            services.AddDbContext<Scheduling.Infrastructure.SchedulingDbContext>(options =>
                options.UseSqlServer(connectionString));
            services.AddDbContext<Pharmacy.Infrastructure.PharmacyDbContext>(options =>
                options.UseSqlServer(connectionString));
            services.AddDbContext<Optical.Infrastructure.OpticalDbContext>(options =>
                options.UseSqlServer(connectionString));
            services.AddDbContext<BillingDbContext>(options =>
                options.UseSqlServer(connectionString));
            services.AddDbContext<Treatment.Infrastructure.TreatmentDbContext>(options =>
                options.UseSqlServer(connectionString));

            // PasswordHasher needed for user seeding
            services.AddSingleton<PasswordHasher>();
        });

        // Create database schema before host starts
        EnsureTestDatabaseCreated(connectionString);
    }

    private static void EnsureTestDatabaseCreated(string connectionString)
    {
        // First context creates the database + its tables
        using (var ctx = CreateDbContext<AuthDbContext>(connectionString))
            ctx.Database.EnsureCreated();

        // Subsequent contexts: database exists, explicitly create their tables
        CreateTablesIfNotExist<Audit.Infrastructure.AuditDbContext>(connectionString);
        CreateTablesIfNotExist<Shared.Infrastructure.ReferenceDbContext>(connectionString);
        CreateTablesIfNotExist<Patient.Infrastructure.PatientDbContext>(connectionString);
        CreateTablesIfNotExist<Scheduling.Infrastructure.SchedulingDbContext>(connectionString);
        CreateTablesIfNotExist<Pharmacy.Infrastructure.PharmacyDbContext>(connectionString);
        CreateTablesIfNotExist<Optical.Infrastructure.OpticalDbContext>(connectionString);
        CreateTablesIfNotExist<Clinical.Infrastructure.ClinicalDbContext>(connectionString);
        CreateTablesIfNotExist<BillingDbContext>(connectionString);
        CreateTablesIfNotExist<Treatment.Infrastructure.TreatmentDbContext>(connectionString);
    }

    private static void CreateTablesIfNotExist<TContext>(string connectionString) where TContext : DbContext
    {
        using var ctx = CreateDbContext<TContext>(connectionString);
        try
        {
            var creator = ctx.GetService<IRelationalDatabaseCreator>();
            creator.CreateTables();
        }
        catch (Microsoft.Data.SqlClient.SqlException)
        {
            // Tables may already exist -- ignore
        }
    }

    private static TContext CreateDbContext<TContext>(string connectionString) where TContext : DbContext
    {
        var optionsBuilder = new DbContextOptionsBuilder<TContext>();
        optionsBuilder.UseSqlServer(connectionString);
        return (TContext)Activator.CreateInstance(typeof(TContext), optionsBuilder.Options)!;
    }

    public async Task InitializeAsync()
    {
        // Force the host to fully start (triggers Wolverine initialization)
        _ = CreateClient();

        using var scope = Services.CreateScope();
        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var billingDb = scope.ServiceProvider.GetRequiredService<BillingDbContext>();

        var branchId = new BranchId(Guid.Parse("00000000-0000-0000-0000-000000000001"));

        // Seed permissions (all Module x Action combinations)
        if (!await authDb.Permissions.AnyAsync())
        {
            var permissions = new List<Permission>();
            foreach (var module in Enum.GetValues<PermissionModule>())
            {
                foreach (var action in Enum.GetValues<PermissionAction>())
                {
                    permissions.Add(new Permission(module, action, $"{action} access to {module}"));
                }
            }
            authDb.Permissions.AddRange(permissions);
            await authDb.SaveChangesAsync();
        }

        // Seed Admin role with all permissions
        if (!await authDb.Roles.AnyAsync())
        {
            var allPerms = await authDb.Permissions.ToListAsync();
            var adminRole = new Role("Admin", "Full system administrator", true, branchId);
            adminRole.UpdatePermissions(allPerms);
            authDb.Roles.Add(adminRole);
            await authDb.SaveChangesAsync();
        }

        // Seed system settings for token lifetimes
        if (!await authDb.SystemSettings.AnyAsync())
        {
            authDb.SystemSettings.AddRange(
                new SystemSetting("AccessTokenLifetimeMinutes", "15", "JWT access token lifetime"),
                new SystemSetting("RefreshTokenLifetimeDays", "7", "Refresh token lifetime"),
                new SystemSetting("RememberMeRefreshTokenLifetimeDays", "30", "Remember me token lifetime"),
                new SystemSetting("SessionTimeoutMinutes", "30", "Session timeout"));
            await authDb.SaveChangesAsync();
        }

        // Seed test user with Admin role
        User? user;
        if (!await authDb.Users.AnyAsync(u => u.Email == TestUserEmail))
        {
            var passwordHasher = new PasswordHasher();
            var passwordHash = passwordHasher.HashPassword(TestUserPassword);
            user = User.Create(TestUserEmail, TestUserFullName, passwordHash, branchId);

            var role = await authDb.Roles.FirstAsync(r => r.Name == "Admin");
            user.AssignRole(role);

            authDb.Users.Add(user);
            await authDb.SaveChangesAsync();
        }
        else
        {
            user = await authDb.Users.FirstAsync(u => u.Email == TestUserEmail);
        }

        // Seed a finalized invoice with line items and payment for print tests
        var invoice = Invoice.Create(
            invoiceNumber: "INV-TEST-001",
            patientId: Guid.NewGuid(),
            patientName: "Test Patient",
            visitId: Guid.NewGuid(),
            branchId: branchId);

        invoice.AddLineItem(
            description: "Eye Examination",
            descriptionVi: "Kham mat",
            unitPrice: 300000m,
            quantity: 1,
            department: Department.Medical);

        invoice.AddLineItem(
            description: "Eye Drops",
            descriptionVi: "Thuoc nho mat",
            unitPrice: 150000m,
            quantity: 2,
            department: Department.Pharmacy);

        // Record a confirmed payment covering the full total
        var payment = Payment.Create(
            invoiceId: invoice.Id,
            method: PaymentMethod.Cash,
            amount: invoice.TotalAmount,
            recordedById: user.Id);
        payment.Confirm();
        invoice.RecordPayment(payment);

        // Finalize the invoice (requires full payment)
        var shiftId = Guid.NewGuid();
        invoice.Finalize(shiftId, user.Id);

        billingDb.Invoices.Add(invoice);
        await billingDb.SaveChangesAsync();

        SeededInvoiceId = invoice.Id;
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        try
        {
            using var ctx = CreateDbContext<AuthDbContext>(TestConnectionString);
            await ctx.Database.EnsureDeletedAsync();
        }
        catch
        {
            // Ignore disposal errors
        }
    }

    private static void RemoveDbContextServices<TContext>(IServiceCollection services) where TContext : DbContext
    {
        var descriptors = services
            .Where(d =>
                d.ServiceType == typeof(DbContextOptions<TContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(TContext) ||
                d.ServiceType == typeof(IDbContextOptionsConfiguration<TContext>))
            .ToList();

        foreach (var descriptor in descriptors)
            services.Remove(descriptor);
    }
}
