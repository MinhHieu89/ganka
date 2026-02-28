using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shared.Application;
using Shared.Application.Services;
using Shared.Infrastructure;
using Shared.Infrastructure.Services;
using Wolverine;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;
using Wolverine.EntityFrameworkCore;
using Wolverine.SqlServer;

// Module DbContexts
using Auth.Infrastructure;
using Audit.Infrastructure;
using Audit.Infrastructure.Interceptors;
using Audit.Infrastructure.Middleware;
using Audit.Infrastructure.Seeding;
using Patient.Infrastructure;
using Clinical.Infrastructure;
using Scheduling.Infrastructure;
using Pharmacy.Infrastructure;
using Optical.Infrastructure;
using Billing.Infrastructure;
using Treatment.Infrastructure;

// Auth services (old -- kept until Plans 03-04 migrate features)
using Auth.Application.Services;
using Auth.Infrastructure.Services;
using Auth.Infrastructure.Seeding;

// New repository/UoW interfaces and implementations
using Auth.Application.Interfaces;
using Auth.Infrastructure.Repositories;
using Audit.Application.Interfaces;

// Presentation layer endpoint extensions
using Auth.Presentation;
using Audit.Presentation;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// ---------------------------------------------------------------------------
// EF Core DbContexts -- one per module, all sharing the same SQL Server
// connection but using schema-per-module isolation.
// ---------------------------------------------------------------------------
// Register AuditInterceptor as a singleton so it can be shared across DbContexts
builder.Services.AddSingleton<AuditInterceptor>();

// AuditDbContext does NOT get the AuditInterceptor (prevents infinite recursion)
builder.Services.AddDbContext<AuditDbContext>(options =>
    options.UseSqlServer(connectionString),
    optionsLifetime: ServiceLifetime.Singleton);

// Register IAuditReadRepository for Application layer query access
builder.Services.AddScoped<IAuditReadRepository>(sp => sp.GetRequiredService<AuditDbContext>());

// All other module DbContexts get the AuditInterceptor for automatic audit logging
void ConfigureDbContext<TContext>(IServiceCollection services) where TContext : DbContext
{
    services.AddDbContext<TContext>((sp, options) =>
    {
        options.UseSqlServer(connectionString);
        options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
    },
    optionsLifetime: ServiceLifetime.Singleton);
}

ConfigureDbContext<AuthDbContext>(builder.Services);
ConfigureDbContext<PatientDbContext>(builder.Services);
ConfigureDbContext<ClinicalDbContext>(builder.Services);
ConfigureDbContext<SchedulingDbContext>(builder.Services);
ConfigureDbContext<PharmacyDbContext>(builder.Services);
ConfigureDbContext<OpticalDbContext>(builder.Services);
ConfigureDbContext<BillingDbContext>(builder.Services);
ConfigureDbContext<TreatmentDbContext>(builder.Services);

// ReferenceDbContext for cross-module reference data (ICD-10 codes, etc.)
builder.Services.AddDbContext<ReferenceDbContext>(options =>
    options.UseSqlServer(connectionString),
    optionsLifetime: ServiceLifetime.Singleton);

// ---------------------------------------------------------------------------
// Authentication -- JWT Bearer
// ---------------------------------------------------------------------------
var jwtSection = builder.Configuration.GetSection("Jwt");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSection["Key"]
                ?? throw new InvalidOperationException("JWT Key not configured."))),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ---------------------------------------------------------------------------
// Auth module services
// ---------------------------------------------------------------------------
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<Auth.Application.Services.IJwtService, JwtService>();
builder.Services.AddScoped<JwtService>(); // Concrete type for AuthService injection
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddHostedService<AuthDataSeeder>();

// ---------------------------------------------------------------------------
// Auth module repositories and UoW (new vertical slice infrastructure)
// Coexists with old service registrations above until Plans 03-04 migrate features.
// ---------------------------------------------------------------------------
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddSingleton<Auth.Application.Interfaces.IPasswordHasher, PasswordHasher>();

// ---------------------------------------------------------------------------
// Shared services
// ---------------------------------------------------------------------------
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IBranchContext, BranchContext>();

// Azure Blob Storage service for medical images and documents
builder.Services.AddScoped<IAzureBlobService, AzureBlobService>();

// ICD-10 ophthalmology code seeder (runs on startup, idempotent)
builder.Services.AddHostedService<Icd10Seeder>();

// ---------------------------------------------------------------------------
// CORS -- allow frontend dev server
// ---------------------------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:3001")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ---------------------------------------------------------------------------
// Swagger / OpenAPI (development only)
// ---------------------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Ganka28 API",
        Version = "v1",
        Description = "Ganka28 Ophthalmology Clinic Management System API"
    });

    // JWT auth support in Swagger UI
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ---------------------------------------------------------------------------
// Wolverine -- message bus, HTTP endpoints, transactional outbox
// ---------------------------------------------------------------------------
builder.Services.AddWolverineHttp();

builder.Host.UseWolverine(opts =>
{
    // Persist messages with SQL Server (transactional outbox)
    opts.PersistMessagesWithSqlServer(connectionString, "wolverine");

    // EF Core transaction middleware for automatic SaveChanges
    opts.UseEntityFrameworkCoreTransactions();

    // Durable local queues for reliability
    opts.Policies.UseDurableLocalQueues();

    // Auto-discover handlers from all module Application assemblies
    opts.Discovery.IncludeAssembly(typeof(Auth.Application.Marker).Assembly);
    opts.Discovery.IncludeAssembly(typeof(Audit.Application.Marker).Assembly);
    opts.Discovery.IncludeAssembly(typeof(Patient.Application.Marker).Assembly);
    opts.Discovery.IncludeAssembly(typeof(Clinical.Application.Marker).Assembly);
    opts.Discovery.IncludeAssembly(typeof(Scheduling.Application.Marker).Assembly);
    opts.Discovery.IncludeAssembly(typeof(Pharmacy.Application.Marker).Assembly);
    opts.Discovery.IncludeAssembly(typeof(Optical.Application.Marker).Assembly);
    opts.Discovery.IncludeAssembly(typeof(Billing.Application.Marker).Assembly);
    opts.Discovery.IncludeAssembly(typeof(Treatment.Application.Marker).Assembly);
});

var app = builder.Build();

// ---------------------------------------------------------------------------
// Middleware pipeline
// ---------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Ganka28 API v1");
    });
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Access logging middleware -- logs all API requests to audit.AccessLogs
app.UseMiddleware<AccessLoggingMiddleware>();

// Wolverine HTTP endpoints with FluentValidation middleware
app.MapWolverineEndpoints(opts =>
{
    opts.UseFluentValidationProblemDetailMiddleware();
});

// Minimal API endpoints (new vertical slice pattern -- coexists with Wolverine endpoints)
app.MapAuthApiEndpoints();
app.MapAuditApiEndpoints();

app.Run();

// Make the implicit Program class accessible for integration testing
public partial class Program;
