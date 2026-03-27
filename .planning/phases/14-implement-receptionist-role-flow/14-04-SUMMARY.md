---
phase: 14-implement-receptionist-role-flow
plan: 04
subsystem: frontend-receptionist
tags: [receptionist, dashboard, kpi, polling, role-based-ui]
dependency_graph:
  requires: [14-02, 14-03]
  provides: [receptionist-dashboard, receptionist-api-client, receptionist-types]
  affects: [dashboard-route, auth-store]
tech_stack:
  added: []
  patterns: [role-based-dashboard-routing, css-variable-theming, tanstack-query-polling]
key_files:
  created:
    - frontend/src/features/receptionist/types/receptionist.types.ts
    - frontend/src/features/receptionist/api/receptionist-api.ts
    - frontend/src/features/receptionist/hooks/useReceptionistPolling.ts
    - frontend/src/features/receptionist/schemas/intake-form.schema.ts
    - frontend/src/features/receptionist/schemas/booking.schema.ts
    - frontend/src/features/receptionist/components/StatusBadge.tsx
    - frontend/src/features/receptionist/components/SourceBadge.tsx
    - frontend/src/features/receptionist/components/KpiCards.tsx
    - frontend/src/features/receptionist/components/StatusFilterPills.tsx
    - frontend/src/features/receptionist/components/PatientQueueTable.tsx
    - frontend/src/features/receptionist/components/ReceptionistDashboard.tsx
  modified:
    - frontend/src/styles/globals.css
    - frontend/src/shared/stores/authStore.ts
    - frontend/src/shared/lib/api-client.ts
    - frontend/src/app/routes/_authenticated.tsx
    - frontend/src/features/auth/hooks/useAuth.ts
    - frontend/src/app/routes/_authenticated/dashboard.tsx
decisions:
  - Added roles field to AuthUser interface to enable role-based dashboard routing
  - Used CSS custom variables for all receptionist theme colors (not Tailwind config)
  - Receptionist dashboard renders at same /dashboard route via role check
metrics:
  duration: 10m
  completed: 2026-03-28
---

# Phase 14 Plan 04: Receptionist Dashboard Frontend Summary

CSS-variable-themed receptionist dashboard with KPI cards (30s polling), patient queue table (15s polling), status filter pills, and role-based route rendering at /dashboard.

## Tasks Completed

### Task 1: CSS variables + types + API client + schemas
**Commit:** 48ef91f

- Added 26 CSS custom variables to globals.css for status badges, KPI, action, and avatar colors
- Created TypeScript types matching backend DTOs: ReceptionistDashboardRow, ReceptionistKpi, AvailableSlot, DashboardFilters
- Created API client with TanStack Query hooks: useReceptionistDashboard (15s poll), useReceptionistKpi (30s poll), useAvailableSlots, plus 9 mutation hooks (check-in, guest booking, no-show, walk-in visit, cancel visit, register/update intake, advance stage)
- Created useManualRefresh polling hook for manual data refresh
- Created Zod schemas: intakeFormSchema (15 fields with Vietnamese validation messages) and bookingSchema (with conditional guest validation)

### Task 2: Dashboard components + role-based routing
**Commit:** d2a3642

- Created StatusBadge with 4 Vietnamese status labels and CSS variable colors, role="status" for accessibility
- Created SourceBadge for appointment/walk-in source display
- Created KpiCards with 4-column grid, skeleton loading states, aria-live="polite" for screen readers
- Created StatusFilterPills using shadcn ToggleGroup (single mode) with count display
- Created PatientQueueTable with TanStack Table: 8 columns (STT, Ho ten, Nam sinh, Gio hen, Nguon, Ly do kham, Trang thai, Thao tac), sorting, pagination (10/20/50), bg-secondary tint for not_arrived rows, Check-in button, empty state message, responsive column hiding
- Created ReceptionistDashboard container: search input, refresh button with tooltip, action buttons (Dat lich hen, Tiep nhan BN moi), error banner for connection loss
- Updated dashboard.tsx route: role check for Receptionist renders ReceptionistDashboard, other roles see DefaultDashboard (existing content preserved)

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical Functionality] Added roles to AuthUser interface**
- **Found during:** Task 1
- **Issue:** AuthUser interface in authStore.ts lacked `roles` field. Backend returns roles in login/refresh responses, but the frontend discarded them. Role-based dashboard routing requires roles.
- **Fix:** Added `roles: string[]` to AuthUser interface and updated all 5 setAuth call sites (authStore.ts, api-client.ts, _authenticated.tsx, useAuth.ts x3) to pass through `response.user.roles`.
- **Files modified:** authStore.ts, api-client.ts, _authenticated.tsx, useAuth.ts
- **Commit:** 48ef91f

## Known Stubs

None - all components are fully wired to API hooks with real data sources. Action menu handler in ReceptionistDashboard is intentionally a no-op placeholder as it will be implemented in plan 14-06 (dashboard actions).

## Self-Check: PASSED
