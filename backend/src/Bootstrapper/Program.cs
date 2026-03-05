using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
using Patient.Infrastructure;
using Clinical.Infrastructure;
using Scheduling.Infrastructure;
using Pharmacy.Infrastructure;
using Optical.Infrastructure;
using Billing.Infrastructure;
using Treatment.Infrastructure;

// Module IoC extension methods
using Auth.Application;
using Auth.Presentation;
using Audit.Application;
using Audit.Presentation;
using Patient.Application;
using Patient.Presentation;
using Clinical.Application;
using Clinical.Presentation;
using Scheduling.Application;
using Scheduling.Presentation;
using Shared.Infrastructure;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// ---------------------------------------------------------------------------
// Module DI registrations (IoC extension methods)
// ---------------------------------------------------------------------------

// Shared services (must come first -- provides HttpContextAccessor etc.)
builder.Services.AddSharedInfrastructure();

// Audit module (must come before Auth -- AuditInterceptor needed by other DbContexts)
builder.Services.AddAuditApplication();
builder.Services.AddAuditInfrastructure(connectionString);
builder.Services.AddAuditPresentation();

// Auth module
builder.Services.AddAuthApplication();
builder.Services.AddAuthInfrastructure();
builder.Services.AddAuthPresentation();

// Patient module
builder.Services.AddPatientApplication();
builder.Services.AddPatientInfrastructure();
builder.Services.AddPatientPresentation();

// Scheduling module
builder.Services.AddSchedulingApplication();
builder.Services.AddSchedulingInfrastructure();
builder.Services.AddSchedulingPresentation();

// Clinical module
builder.Services.AddClinicalApplication();
builder.Services.AddClinicalInfrastructure();
builder.Services.AddClinicalPresentation();

// ---------------------------------------------------------------------------
// EF Core DbContexts -- module DbContexts with AuditInterceptor
// (AuditInterceptor registered above by AddAuditInfrastructure)
// ---------------------------------------------------------------------------
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
// Rate Limiting -- public booking endpoint protection
// ---------------------------------------------------------------------------
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("public-booking", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
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

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("GlobalExceptionHandler");
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        if (exception is not null)
            logger.LogError(exception, "Unhandled exception for {Method} {Path}",
                context.Request.Method, context.Request.Path);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
            title = "Internal Server Error",
            status = 500,
            detail = "An unexpected error occurred. Please try again later."
        });
    });
});

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Rate limiting for public endpoints (must be before endpoint mapping)
app.UseRateLimiter();

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
app.MapPatientApiEndpoints();
app.MapSchedulingApiEndpoints();
app.MapPublicBookingEndpoints();
app.MapPublicOsdiEndpoints();
app.MapClinicalApiEndpoints();

app.Run();

// Make the implicit Program class accessible for integration testing
public partial class Program;
