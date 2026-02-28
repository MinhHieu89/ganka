# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-02-28)

**Core value:** Doctors can manage chronic eye disease patients (Dry Eye, Myopia Control) with structured data tracking, image comparison across visits, and treatment progress reporting
**Current focus:** Phase 1: Foundation & Infrastructure

## Current Position

Phase: 1 of 9 (Foundation & Infrastructure)
Plan: 6 of 7 in current phase
Status: Executing
Last activity: 2026-02-28 -- Completed 01-05 auth UI (login, session, user/role admin)

Progress: [#####.....] 17%

## Performance Metrics

**Velocity:**
- Total plans completed: 5
- Average duration: 12min
- Total execution time: 1.02 hours

**By Phase:**

| Phase | Plans | Total | Avg/Plan |
|-------|-------|-------|----------|
| 1 | 5 | 61min | 12min |

**Recent Trend:**
- Last 5 plans: 10min, 17min, 10min, 15min, 9min
- Trend: stable

*Updated after each plan completion*
| Phase 01 P05 | 9min | 2 tasks | 36 files |

## Accumulated Context

### Decisions

Decisions are logged in PROJECT.md Key Decisions table.
Recent decisions affecting current work:

- [Roadmap]: 9 phases derived from 96 v1 requirements across 18 categories
- [Roadmap]: Build order follows domain dependency chain -- HIS upstream, downstream modules consume events
- [Roadmap]: Three critical pitfalls (generic data model, immutable records, ICD-10 laterality) addressed in Phase 1
- [01-01]: Used .slnx format (modern .NET 10 default) instead of legacy .sln
- [01-01]: Custom Result<T> with implicit T conversion, no Ardalis.Result dependency
- [01-01]: Assembly Marker pattern for Wolverine handler discovery across modules
- [01-01]: FrameworkReference Microsoft.AspNetCore.App on Shared.Infrastructure for claim reading
- [01-02]: TanStack Start v1.163 uses Vite plugin (no vinxi) -- getRouter() export, vite.config.ts
- [01-02]: Stone base + Green accent theme, zero border radius per user decision
- [01-02]: Vietnamese as default language, @tabler/icons-react for all icons
- [01-04]: AuditInterceptor + AccessLoggingMiddleware placed in Audit.Infrastructure (not Shared.Infrastructure) to avoid circular project references
- [01-04]: IAuditReadContext interface for Application-layer DB access without circular dependency
- [01-04]: StorageBlobInfo renamed to avoid Azure.Storage.Blobs.Models.BlobInfo collision
- [01-04]: ReferenceDbContext with "reference" schema for cross-module ICD-10 reference data
- [01-04]: ICD-10 seed data as EmbeddedResource in Audit.Infrastructure assembly
- [Phase 01]: [01-03]: Service interfaces in Application, implementations in Infrastructure to avoid circular dependency
- [Phase 01]: [01-03]: Argon2id via Konscious.Security.Cryptography with 64MB memory, 4 parallelism, 3 iterations
- [Phase 01]: [01-03]: 8 system roles with preset permission templates (Admin, Doctor, Technician, Nurse, Cashier, OpticalStaff, Manager, Accountant)
- [Phase 01]: [01-05]: Dialog component extended with hideCloseButton prop for non-dismissible session warning modal
- [Phase 01]: [01-05]: Session timeout: 30 min inactivity, 2-min warning, activity throttled to 30s intervals
- [Phase 01]: [01-05]: Admin sidebar conditionally rendered based on Auth.Manage or Auth.View permission
- [Phase 01]: [01-05]: Separate React Hook Form instances for create vs edit mode to avoid union type issues

### Pending Todos

None yet.

### Blockers/Concerns

- So Y Te API specification not publicly documented -- must obtain before Phase 7-8 for regulatory compliance (deadline 31/12/2026)
- Zalo OA account not yet created -- should start creation process during Phase 3-4 development (v2 dependency but long lead time)
- TanStack Start is RC stage -- pin versions carefully, plan upgrade task when v1.0 stable ships
- Barcode scanner hardware decision needed before Phase 8 planning

## Session Continuity

Last session: 2026-02-28
Stopped at: Completed 01-05-PLAN.md (auth UI: login, session, user/role admin)
Resume file: .planning/phases/01-foundation-infrastructure/01-05-SUMMARY.md
