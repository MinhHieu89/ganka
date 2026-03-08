---
phase: 08-optical-center
plan: 22
subsystem: optical-infrastructure
tags: [ioc, di-registration, seeder, repositories, suppliers]
dependency_graph:
  requires: [08-11, 08-12, 08-13, 08-14, 08-38]
  provides: [optical-di-wiring, optical-supplier-seeding]
  affects: [bootstrapper, optical-module-startup]
tech_stack:
  added: []
  patterns: [ioc-extension-methods, ihostedservice-seeder, cross-module-db-access]
key_files:
  created:
    - backend/src/Modules/Optical/Optical.Infrastructure/Seeding/OpticalSupplierSeeder.cs
    - backend/src/Modules/Optical/Optical.Infrastructure/Repositories/LensCatalogRepository.cs
    - backend/src/Modules/Optical/Optical.Infrastructure/UnitOfWork.cs
  modified:
    - backend/src/Modules/Optical/Optical.Application/IoC.cs
    - backend/src/Modules/Optical/Optical.Infrastructure/IoC.cs
    - backend/src/Modules/Optical/Optical.Infrastructure/Optical.Infrastructure.csproj
decisions:
  - "Used PharmacyDbContext directly for seeder (fallback) instead of Pharmacy.Contracts IMessageBus pattern since CreateSupplier command not yet exposed in Pharmacy.Contracts"
  - "Added Pharmacy.Infrastructure ProjectReference to Optical.Infrastructure.csproj for cross-module seeder access"
metrics:
  duration: "~20 minutes"
  completed_date: "2026-03-08"
  tasks_completed: 2
  files_changed: 6
---

# Phase 08 Plan 22: Optical Module IoC Registrations and Supplier Seeder Summary

## One-liner

DI registration for all 6 Optical repositories + UnitOfWork + OpticalSupplierSeeder that idempotently seeds Essilor Vietnam, Hoya Lens Vietnam, and Kinh mat Viet Phap with SupplierType.Optical flag.

## Tasks Completed

| Task | Description | Status | Commit |
|------|-------------|--------|--------|
| 1 | Application and Infrastructure IoC registrations | Done | 39d4513 |
| 2 | OpticalSupplierSeeder using PharmacyDbContext cross-module access | Done | 0211016 |

## What Was Built

### Application IoC (Optical.Application/IoC.cs)

`AddOpticalApplication` registers FluentValidation validators from the Optical.Application assembly:

```csharp
public static IServiceCollection AddOpticalApplication(this IServiceCollection services)
{
    services.AddValidatorsFromAssembly(typeof(Marker).Assembly, ServiceLifetime.Scoped);
    return services;
}
```

### Infrastructure IoC (Optical.Infrastructure/IoC.cs)

`AddOpticalInfrastructure` registers all 6 repositories, UnitOfWork, and the OpticalSupplierSeeder:

```csharp
services.AddScoped<IFrameRepository, FrameRepository>();
services.AddScoped<ILensCatalogRepository, LensCatalogRepository>();
services.AddScoped<IGlassesOrderRepository, GlassesOrderRepository>();
services.AddScoped<IComboPackageRepository, ComboPackageRepository>();
services.AddScoped<IWarrantyClaimRepository, WarrantyClaimRepository>();
services.AddScoped<IStocktakingRepository, StocktakingRepository>();
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddHostedService<OpticalSupplierSeeder>();
```

### OpticalSupplierSeeder

IHostedService that seeds 3 optical suppliers at startup:
- **Essilor Vietnam** (ContactInfo: Ho Chi Minh City) - SupplierType.Optical
- **Hoya Lens Vietnam** (ContactInfo: Ho Chi Minh City) - SupplierType.Optical
- **Kinh mat Viet Phap** (ContactInfo: Ho Chi Minh City) - SupplierType.Optical

Idempotent: checks existing suppliers by name before inserting. If a supplier exists without the Optical flag, it adds it using bitwise OR (`existing | SupplierType.Optical`).

### LensCatalogRepository + UnitOfWork

Created `LensCatalogRepository` (ILensCatalogRepository) with eager StockEntries loading and power-combination query. Created `UnitOfWork` wrapping OpticalDbContext.SaveChangesAsync.

## Verification

Build result: `dotnet build Optical.Infrastructure.csproj` - **Build succeeded. 0 warnings, 0 errors.**

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Dependency plans not yet executed**
- **Found during:** Task 1
- **Issue:** Plans 08-16 through 08-21 and 08-38 (repository interfaces, implementations, SupplierType) had not been executed when this plan ran
- **Fix:** All dependency artifacts were already committed by earlier plan executions (08-11, 08-12, 08-13, 08-14, 08-38) that ran in parallel or out-of-order. No additional work needed.
- **Files modified:** N/A (artifacts already existed in git)

**2. [Rule 2 - Architecture Note] PharmacyDbContext fallback for seeder**
- **Found during:** Task 2
- **Issue:** Pharmacy.Contracts does not expose a `CreateSupplier` command; IMessageBus cross-module pattern not available
- **Fix:** Used PharmacyDbContext direct access (as explicitly allowed by the plan note) with a TODO comment to migrate to Contracts pattern when available
- **Files modified:** OpticalSupplierSeeder.cs, Optical.Infrastructure.csproj

## Self-Check: PASSED

All required files exist and commits are confirmed:
- FOUND: `backend/src/Modules/Optical/Optical.Infrastructure/IoC.cs` (committed in 39d4513)
- FOUND: `backend/src/Modules/Optical/Optical.Application/IoC.cs` (committed in 8f299a6)
- FOUND: `backend/src/Modules/Optical/Optical.Infrastructure/Seeding/OpticalSupplierSeeder.cs` (committed in 0211016)
- FOUND: `backend/src/Modules/Optical/Optical.Infrastructure/Repositories/LensCatalogRepository.cs` (committed in 6397941)
- FOUND: `backend/src/Modules/Optical/Optical.Infrastructure/UnitOfWork.cs` (committed in 6397941)
- Build: `Optical.Infrastructure.csproj` - Build succeeded. 0 warnings, 0 errors.
