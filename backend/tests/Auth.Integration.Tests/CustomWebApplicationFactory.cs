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
using Shared.Domain;

namespace Auth.Integration.Tests;

/// <summary>
/// Custom WebApplicationFactory for Auth integration tests.
/// Uses a dedicated test database (LocalDB) with a unique name per test run
/// to avoid polluting the development database.
/// Seeds a test user for login/refresh/logout endpoint testing.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string TestUserEmail = "testuser@ganka28.com";
    public const string TestUserPassword = "TestPassword@123";
    public const string TestUserFullName = "Test User";

    private readonly string _testDbName = $"Ganka28_IntTest_{Guid.NewGuid():N}";

    private string TestConnectionString =>
        $"Server=(localdb)\\mssqllocaldb;Database={_testDbName};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        var connectionString = TestConnectionString;

        builder.ConfigureTestServices(services =>
        {
            // Remove all DbContext registrations
            RemoveDbContextServices<AuthDbContext>(services);
            RemoveDbContextServices<Audit.Infrastructure.AuditDbContext>(services);
            RemoveDbContextServices<Shared.Infrastructure.ReferenceDbContext>(services);
            RemoveDbContextServices<Patient.Infrastructure.PatientDbContext>(services);
            RemoveDbContextServices<Clinical.Infrastructure.ClinicalDbContext>(services);
            RemoveDbContextServices<Scheduling.Infrastructure.SchedulingDbContext>(services);
            RemoveDbContextServices<Pharmacy.Infrastructure.PharmacyDbContext>(services);
            RemoveDbContextServices<Optical.Infrastructure.OpticalDbContext>(services);
            RemoveDbContextServices<Billing.Infrastructure.BillingDbContext>(services);
            RemoveDbContextServices<Treatment.Infrastructure.TreatmentDbContext>(services);

            // Re-register all DbContexts with test database (no AuditInterceptor)
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
            services.AddDbContext<Billing.Infrastructure.BillingDbContext>(options =>
                options.UseSqlServer(connectionString));
            services.AddDbContext<Treatment.Infrastructure.TreatmentDbContext>(options =>
                options.UseSqlServer(connectionString));

            // AuthDataSeeder resolves concrete PasswordHasher -- register it explicitly
            services.AddSingleton<PasswordHasher>();
        });

        // Create the database schema BEFORE the host starts (seeders run on StartAsync)
        EnsureTestDatabaseCreated(connectionString);
    }

    /// <summary>
    /// Create all tables before the host starts so that hosted services (AuthDataSeeder, etc.) can access them.
    /// Uses EnsureCreated for the first context (creates the DB), then CreateTables for subsequent contexts
    /// since EnsureCreated is a no-op once the database exists.
    /// </summary>
    private static void EnsureTestDatabaseCreated(string connectionString)
    {
        // First context creates the database + its tables
        using (var ctx = CreateDbContext<AuthDbContext>(connectionString))
            ctx.Database.EnsureCreated();

        // Subsequent contexts: database exists, so we need to explicitly create their tables
        CreateTablesIfNotExist<Audit.Infrastructure.AuditDbContext>(connectionString);
        CreateTablesIfNotExist<Shared.Infrastructure.ReferenceDbContext>(connectionString);
        CreateTablesIfNotExist<Patient.Infrastructure.PatientDbContext>(connectionString);
        CreateTablesIfNotExist<Scheduling.Infrastructure.SchedulingDbContext>(connectionString);
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
        // Access Services to trigger host startup (seeders will run)
        using var scope = Services.CreateScope();
        var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        // Seed test user if not exists
        if (!await authDb.Users.AnyAsync(u => u.Email == TestUserEmail))
        {
            var passwordHasher = new PasswordHasher();
            var passwordHash = passwordHasher.HashPassword(TestUserPassword);
            var branchId = new BranchId(Guid.Parse("00000000-0000-0000-0000-000000000001"));
            var user = User.Create(TestUserEmail, TestUserFullName, passwordHash, branchId);
            authDb.Users.Add(user);
            await authDb.SaveChangesAsync();
        }
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        // Clean up test database
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
                d.ServiceType == typeof(TContext))
            .ToList();

        foreach (var descriptor in descriptors)
            services.Remove(descriptor);
    }
}
