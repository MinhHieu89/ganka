---
phase: 01-foundation-infrastructure
plan: 04
subsystem: audit
tags: [ef-core-interceptor, audit-logging, access-logging, azure-blob, acl-adapter, icd10, template-engine, data-export]

# Dependency graph
requires:
  - phase: 01-01
    provides: "Shared DDD kernel (IAuditable, Entity, Result<T>), module scaffolds, Bootstrapper, AuditDbContext"
provides:
  - "AuditInterceptor capturing field-level old/new values on IAuditable entities"
  - "AccessLoggingMiddleware logging all API HTTP requests to audit.AccessLogs"
  - "AuditDbContext with AuditLogs and AccessLogs tables in audit schema"
  - "Wolverine.HTTP endpoints for admin audit/access log querying with cursor-based pagination"
  - "CSV export endpoint for audit log compliance reporting"
  - "IAzureBlobService interface and AzureBlobService implementation"
  - "IExternalSystemAdapter<TRequest,TResponse> ACL adapter pattern"
  - "ITemplateDefinition + ITemplateRegistry for disease-specific templates"
  - "IDataExportService + ExportFormat for full data export"
  - "Icd10Code entity + ReferenceDbContext with reference schema"
  - "120 ICD-10 ophthalmology codes seeded via Icd10Seeder hosted service"
  - "IAuditReadContext interface for clean Application-layer DB access"
affects: [01-05, 01-06, 01-07, 02-01, 03-01, 04-01, 06-01, 07-01]

# Tech tracking
tech-stack:
  added: ["Azure.Storage.Blobs 12.x (in Shared.Infrastructure)", "EF Core SaveChangesInterceptor pattern"]
  patterns: ["AuditInterceptor on all module DbContexts for automatic audit logging", "IAuditReadContext interface to avoid circular dependency", "StorageBlobInfo renamed to avoid Azure SDK collision", "Icd10Seeder IHostedService with embedded JSON resource", "ReferenceDbContext for cross-module reference data"]

key-files:
  created:
    - "backend/src/Modules/Audit/Audit.Domain/Entities/AuditLog.cs"
    - "backend/src/Modules/Audit/Audit.Domain/Entities/AccessLog.cs"
    - "backend/src/Modules/Audit/Audit.Domain/Enums/AuditAction.cs"
    - "backend/src/Modules/Audit/Audit.Domain/Enums/AccessAction.cs"
    - "backend/src/Modules/Audit/Audit.Contracts/Dtos/AuditLogDto.cs"
    - "backend/src/Modules/Audit/Audit.Contracts/Dtos/AuditLogQuery.cs"
    - "backend/src/Modules/Audit/Audit.Contracts/Dtos/AccessLogDto.cs"
    - "backend/src/Modules/Audit/Audit.Application/Endpoints/AuditLogEndpoints.cs"
    - "backend/src/Modules/Audit/Audit.Application/Endpoints/AccessLogEndpoints.cs"
    - "backend/src/Modules/Audit/Audit.Application/IAuditReadContext.cs"
    - "backend/src/Modules/Audit/Audit.Infrastructure/Configurations/AuditLogConfiguration.cs"
    - "backend/src/Modules/Audit/Audit.Infrastructure/Configurations/AccessLogConfiguration.cs"
    - "backend/src/Modules/Audit/Audit.Infrastructure/Interceptors/AuditInterceptor.cs"
    - "backend/src/Modules/Audit/Audit.Infrastructure/Middleware/AccessLoggingMiddleware.cs"
    - "backend/src/Modules/Audit/Audit.Infrastructure/Seeding/Icd10Seeder.cs"
    - "backend/src/Modules/Audit/Audit.Infrastructure/Seeding/icd10-ophthalmology.json"
    - "backend/src/Shared/Shared.Application/Services/IAzureBlobService.cs"
    - "backend/src/Shared/Shared.Infrastructure/Services/AzureBlobService.cs"
    - "backend/src/Shared/Shared.Infrastructure/ReferenceDbContext.cs"
    - "backend/src/Shared/Shared.Domain/ITemplateDefinition.cs"
    - "backend/src/Shared/Shared.Domain/Icd10Code.cs"
    - "backend/src/Shared/Shared.Domain/Ports/IExternalSystemAdapter.cs"
    - "backend/src/Shared/Shared.Contracts/IDataExportService.cs"
  modified:
    - "backend/src/Modules/Audit/Audit.Infrastructure/AuditDbContext.cs"
    - "backend/src/Modules/Audit/Audit.Application/Audit.Application.csproj"
    - "backend/src/Modules/Audit/Audit.Infrastructure/Audit.Infrastructure.csproj"
    - "backend/src/Shared/Shared.Infrastructure/Shared.Infrastructure.csproj"
    - "backend/src/Bootstrapper/Program.cs"
    - "backend/src/Bootstrapper/appsettings.json"

key-decisions:
  - "Moved AuditInterceptor and AccessLoggingMiddleware to Audit.Infrastructure instead of Shared.Infrastructure to avoid circular project references"
  - "Created IAuditReadContext interface in Audit.Application to allow endpoints to query AuditDbContext without Infrastructure dependency"
  - "Renamed BlobInfo to StorageBlobInfo to avoid naming collision with Azure.Storage.Blobs.Models.BlobInfo"
  - "Placed ICD-10 codes in ReferenceDbContext (reference schema) as cross-module reference data"
  - "ICD-10 seed data embedded as JSON resource in assembly for reliable deployment"

patterns-established:
  - "AuditInterceptor registered on all module DbContexts (except AuditDbContext itself) for automatic field-level audit logging"
  - "IAuditReadContext interface pattern for Application-layer read-only queries without circular dependency"
  - "IExternalSystemAdapter<TRequest, TResponse> ACL adapter pattern for external system integration"
  - "ITemplateDefinition + ITemplateRegistry for extensible disease-specific clinical templates"
  - "ReferenceDbContext with reference schema for cross-module lookup data"
  - "Icd10Seeder IHostedService pattern for idempotent startup seeding from embedded resources"
  - "Cursor-based pagination (Timestamp + Id) for audit log queries"

requirements-completed: [AUTH-05, AUD-01, AUD-02, AUD-03, AUD-04, ARC-01, ARC-03, ARC-04, ARC-05, ARC-06]

# Metrics
duration: 15min
completed: 2026-02-28
---

# Phase 1 Plan 04: Audit Module and Architecture Foundations Summary

**EF Core AuditInterceptor with field-level change tracking, AccessLoggingMiddleware, 120 ICD-10 ophthalmology codes, Azure Blob service, ACL adapter pattern, template engine and data export interfaces**

## Performance

- **Duration:** 15 min
- **Started:** 2026-02-28T13:42:05Z
- **Completed:** 2026-02-28T13:57:15Z
- **Tasks:** 2
- **Files modified:** 32

## Accomplishments
- Built AuditInterceptor (SaveChangesInterceptor) that automatically captures field-level old/new values on all IAuditable entities across all module DbContexts, writing to audit.AuditLogs
- Created AccessLoggingMiddleware that logs all API HTTP requests to audit.AccessLogs with fire-and-forget pattern
- Implemented cursor-based pagination audit log query endpoints (GET /api/admin/audit-logs) and CSV export endpoint (GET /api/admin/audit-logs/export)
- Created Azure Blob Storage service (IAzureBlobService/AzureBlobService) for medical image and document storage
- Established ACL adapter pattern (IExternalSystemAdapter<TRequest, TResponse>) for future external integrations
- Scaffolded template engine interface (ITemplateDefinition/ITemplateRegistry) for disease-specific clinical templates
- Defined data export service interface (IDataExportService/ExportFormat) for full data ownership
- Seeded 120 ICD-10 ophthalmology codes with bilingual (EN/VI) descriptions covering Dry Eye, Myopia, Keratoconus, Glaucoma, Conjunctivitis, Blepharitis, Cataract, Macular Degeneration, Diabetic Retinopathy, and more

## Task Commits

Each task was committed atomically:

1. **Task 1: Audit domain entities, DbContext, EF interceptor, and access logging middleware** - `79114b9` (feat)
2. **Task 2: Architecture foundations -- Azure Blob, ACL adapters, template engine, data export, ICD-10 seeding** - `72b62e9` (feat)

## Files Created/Modified
- `backend/src/Modules/Audit/Audit.Domain/Entities/AuditLog.cs` - Immutable audit log entity with factory method
- `backend/src/Modules/Audit/Audit.Domain/Entities/AccessLog.cs` - HTTP access log entity
- `backend/src/Modules/Audit/Audit.Domain/Enums/AuditAction.cs` - Created/Updated/Deleted enum
- `backend/src/Modules/Audit/Audit.Domain/Enums/AccessAction.cs` - Login/LoginFailed/Logout/ViewRecord/ApiRequest enum
- `backend/src/Modules/Audit/Audit.Contracts/Dtos/*.cs` - DTOs for audit/access log API responses
- `backend/src/Modules/Audit/Audit.Application/IAuditReadContext.cs` - Read-only query interface
- `backend/src/Modules/Audit/Audit.Application/Endpoints/AuditLogEndpoints.cs` - Audit log query + CSV export endpoints
- `backend/src/Modules/Audit/Audit.Application/Endpoints/AccessLogEndpoints.cs` - Access log query endpoint
- `backend/src/Modules/Audit/Audit.Infrastructure/AuditDbContext.cs` - Updated with DbSets, configurations, IAuditReadContext
- `backend/src/Modules/Audit/Audit.Infrastructure/Configurations/*.cs` - EF Core entity configurations with indexes
- `backend/src/Modules/Audit/Audit.Infrastructure/Interceptors/AuditInterceptor.cs` - SaveChangesInterceptor for field-level audit
- `backend/src/Modules/Audit/Audit.Infrastructure/Middleware/AccessLoggingMiddleware.cs` - HTTP request logging
- `backend/src/Modules/Audit/Audit.Infrastructure/Seeding/icd10-ophthalmology.json` - 120 ICD-10 codes
- `backend/src/Modules/Audit/Audit.Infrastructure/Seeding/Icd10Seeder.cs` - Startup seeder hosted service
- `backend/src/Shared/Shared.Application/Services/IAzureBlobService.cs` - Blob storage port interface
- `backend/src/Shared/Shared.Infrastructure/Services/AzureBlobService.cs` - Blob storage adapter
- `backend/src/Shared/Shared.Infrastructure/ReferenceDbContext.cs` - Cross-module reference data context
- `backend/src/Shared/Shared.Domain/ITemplateDefinition.cs` - Template engine interfaces
- `backend/src/Shared/Shared.Domain/Icd10Code.cs` - ICD-10 reference code entity
- `backend/src/Shared/Shared.Domain/Ports/IExternalSystemAdapter.cs` - ACL adapter pattern
- `backend/src/Shared/Shared.Contracts/IDataExportService.cs` - Data export interface
- `backend/src/Bootstrapper/Program.cs` - Registered all new services, interceptor, middleware

## Decisions Made
- **Moved interceptor/middleware to Audit.Infrastructure:** Plan specified Shared.Infrastructure but this created circular project references (Shared.Infrastructure -> Audit.Infrastructure -> Shared.Infrastructure). Moved AuditInterceptor and AccessLoggingMiddleware to Audit.Infrastructure where they naturally belong alongside AuditDbContext.
- **Created IAuditReadContext interface:** To allow Audit.Application endpoints to query the database without a circular dependency to Audit.Infrastructure. AuditDbContext implements IAuditReadContext; registered as scoped DI service.
- **Renamed BlobInfo to StorageBlobInfo:** Azure.Storage.Blobs.Models already defines a BlobInfo class, causing ambiguous reference compile errors. Renamed our DTO to StorageBlobInfo.
- **ICD-10 seed as embedded resource:** Used EmbeddedResource in .csproj to ensure the JSON file is available regardless of deployment directory structure. Seeder falls back to file path if embedded resource not found.
- **ReferenceDbContext in Shared.Infrastructure:** ICD-10 codes are cross-module reference data. A dedicated ReferenceDbContext with "reference" schema keeps them separate from any single module's data.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Moved AuditInterceptor and AccessLoggingMiddleware from Shared.Infrastructure to Audit.Infrastructure**
- **Found during:** Task 1 (Interceptor implementation)
- **Issue:** Placing the interceptor and middleware in Shared.Infrastructure created a circular project reference: Shared.Infrastructure -> Audit.Infrastructure -> Shared.Infrastructure
- **Fix:** Moved both files to Audit.Infrastructure/Interceptors/ and Audit.Infrastructure/Middleware/ namespaces
- **Files modified:** Audit.Infrastructure.csproj (added FrameworkReference, Shared.Application/Domain refs), Shared.Infrastructure.csproj (removed compile excludes)
- **Verification:** Build succeeds with 0 errors, no circular dependencies
- **Committed in:** 79114b9 (Task 1 commit)

**2. [Rule 3 - Blocking] Created IAuditReadContext interface to break circular dependency**
- **Found during:** Task 1 (Endpoint implementation)
- **Issue:** Audit.Application endpoints needed AuditDbContext (in Infrastructure), but Infrastructure already references Application -- circular dependency
- **Fix:** Created IAuditReadContext interface in Audit.Application, implemented by AuditDbContext in Infrastructure. Endpoints use the interface.
- **Files modified:** Audit.Application/IAuditReadContext.cs (new), AuditDbContext.cs (implements IAuditReadContext), Program.cs (registers IAuditReadContext)
- **Verification:** Build succeeds, endpoints can query audit data
- **Committed in:** 79114b9 (Task 1 commit)

**3. [Rule 1 - Bug] Renamed BlobInfo to StorageBlobInfo to avoid naming collision**
- **Found during:** Task 2 (Azure Blob service)
- **Issue:** Azure.Storage.Blobs.Models.BlobInfo collides with our Shared.Application.Services.BlobInfo causing CS0104 ambiguous reference
- **Fix:** Renamed our DTO to StorageBlobInfo in both IAzureBlobService.cs and AzureBlobService.cs
- **Files modified:** IAzureBlobService.cs, AzureBlobService.cs
- **Verification:** Build succeeds with 0 errors
- **Committed in:** 72b62e9 (Task 2 commit)

**4. [Rule 1 - Bug] Fixed GetBlobsAsync API call signature**
- **Found during:** Task 2 (Azure Blob service)
- **Issue:** Azure.Storage.Blobs 12.x GetBlobsAsync requires explicit BlobTraits, BlobStates, and CancellationToken parameters
- **Fix:** Changed to `GetBlobsAsync(BlobTraits.Metadata, BlobStates.None, prefix, CancellationToken.None)`
- **Files modified:** AzureBlobService.cs
- **Verification:** Build succeeds with 0 errors
- **Committed in:** 72b62e9 (Task 2 commit)

---

**Total deviations:** 4 auto-fixed (2 blocking, 2 bug)
**Impact on plan:** All auto-fixes necessary for the solution to compile and avoid circular dependencies. No scope creep.

## Issues Encountered
- Linter/automated process kept re-adding Compile Remove entries to .csproj files for newly created files, requiring multiple corrections before the build could succeed.

## User Setup Required

None - no external service configuration required. Azure Storage uses development storage emulator by default.

## Next Phase Readiness
- Audit interceptor automatically captures all entity changes -- ready for Phase 2+ domain entities that implement IAuditable
- Access logging middleware active on all API requests
- Azure Blob service registered and injectable -- ready for medical image storage in Phase 4
- ACL adapter pattern established -- ready for Zalo OA (Phase 3), MISA (Phase 6), So Y Te (Phase 7)
- Template engine interfaces scaffolded -- ready for Dry Eye template (Phase 4)
- ICD-10 ophthalmology codes seeded and queryable -- ready for diagnosis coding in Phase 2-3
- All architecture foundation requirements satisfied

## Self-Check: PASSED

All key files exist on disk. Both task commits verified (79114b9, 72b62e9). All minimum line count requirements met (AuditInterceptor: 244, AccessLoggingMiddleware: 123, AzureBlobService: 130, icd10-ophthalmology.json: 1059). Key links verified: AuditInterceptor references AuditDbContext/AuditLog, AccessLoggingMiddleware references AccessLog, AuditDbContext has "audit" schema. Solution builds with 0 errors.

---
*Phase: 01-foundation-infrastructure*
*Completed: 2026-02-28*
