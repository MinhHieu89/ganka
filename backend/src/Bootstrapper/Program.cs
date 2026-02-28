using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shared.Application;
using Shared.Infrastructure;
using Wolverine;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;
using Wolverine.EntityFrameworkCore;
using Wolverine.SqlServer;

// Module DbContexts
using Auth.Infrastructure;
using Audit.Infrastructure;
using Patient.Infrastructure;
using Clinical.Infrastructure;
using Scheduling.Infrastructure;
using Pharmacy.Infrastructure;
using Optical.Infrastructure;
using Billing.Infrastructure;
using Treatment.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// ---------------------------------------------------------------------------
// EF Core DbContexts -- one per module, all sharing the same SQL Server
// connection but using schema-per-module isolation.
// ---------------------------------------------------------------------------
void ConfigureDbContext<TContext>(IServiceCollection services) where TContext : DbContext
{
    services.AddDbContext<TContext>(options =>
        options.UseSqlServer(connectionString),
        optionsLifetime: ServiceLifetime.Singleton);
}

ConfigureDbContext<AuthDbContext>(builder.Services);
ConfigureDbContext<AuditDbContext>(builder.Services);
ConfigureDbContext<PatientDbContext>(builder.Services);
ConfigureDbContext<ClinicalDbContext>(builder.Services);
ConfigureDbContext<SchedulingDbContext>(builder.Services);
ConfigureDbContext<PharmacyDbContext>(builder.Services);
ConfigureDbContext<OpticalDbContext>(builder.Services);
ConfigureDbContext<BillingDbContext>(builder.Services);
ConfigureDbContext<TreatmentDbContext>(builder.Services);

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
// Shared services
// ---------------------------------------------------------------------------
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IBranchContext, BranchContext>();

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

app.UseAuthentication();
app.UseAuthorization();

// Wolverine HTTP endpoints with FluentValidation middleware
app.MapWolverineEndpoints(opts =>
{
    opts.UseFluentValidationProblemDetailMiddleware();
});

app.Run();

// Make the implicit Program class accessible for integration testing
public partial class Program;
