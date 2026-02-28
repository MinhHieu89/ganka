---
phase: 01-foundation-infrastructure
plan: 05
subsystem: ui
tags: [react, tanstack-query, tanstack-table, react-hook-form, zod, shadcn-ui, zustand, i18next, jwt, session-management, rbac]

# Dependency graph
requires:
  - phase: 01-02
    provides: "TanStack Start SPA scaffold, shadcn/ui components, i18next bilingual, auth store, openapi-fetch client, app shell layout"
  - phase: 01-03
    provides: "Auth backend endpoints (login, refresh, logout, user CRUD, role CRUD, permissions, language), DTOs"
provides:
  - "Functional login page with split layout, form validation, and JWT auth flow"
  - "Session management with activity tracking, timeout warning modal, and auto-extend"
  - "Automatic token refresh at 80% of access token lifetime"
  - "User management admin page with create/edit dialog and role assignment"
  - "Role management page with permission matrix (10 modules x 6 actions)"
  - "Permission-gated sidebar navigation for admin section"
  - "Language preference persistence to backend on toggle"
affects: [01-06, 01-07, 02, 03, 04, 05, 06, 07, 08, 09]

# Tech tracking
tech-stack:
  added: []
  patterns: ["Feature-based API layer pattern (features/*/api/*.ts with TanStack Query hooks)", "useAuth hook encapsulating login/logout/refresh with auto token scheduling", "useSession hook with activity-based session timeout and warning modal", "Permission matrix UI pattern (checkboxes grouped by module with Select All per row/column)", "Conditional sidebar navigation based on user permissions"]

key-files:
  created:
    - "frontend/src/features/auth/api/auth-api.ts"
    - "frontend/src/features/auth/hooks/useAuth.ts"
    - "frontend/src/features/auth/hooks/useSession.ts"
    - "frontend/src/features/auth/components/LoginForm.tsx"
    - "frontend/src/features/auth/components/LoginPage.tsx"
    - "frontend/src/features/auth/components/SessionWarningModal.tsx"
    - "frontend/src/features/admin/api/admin-api.ts"
    - "frontend/src/features/admin/hooks/useUsers.ts"
    - "frontend/src/features/admin/hooks/useRoles.ts"
    - "frontend/src/features/admin/components/UserManagementPage.tsx"
    - "frontend/src/features/admin/components/UserTable.tsx"
    - "frontend/src/features/admin/components/UserFormDialog.tsx"
    - "frontend/src/features/admin/components/RoleManagementPage.tsx"
    - "frontend/src/features/admin/components/RoleTable.tsx"
    - "frontend/src/features/admin/components/PermissionMatrix.tsx"
    - "frontend/src/app/routes/_authenticated/admin/users.tsx"
    - "frontend/src/app/routes/_authenticated/admin/roles.tsx"
  modified:
    - "frontend/src/app/routes/login.tsx"
    - "frontend/src/shared/components/AppShell.tsx"
    - "frontend/src/shared/components/AppSidebar.tsx"
    - "frontend/src/shared/components/TopBar.tsx"
    - "frontend/src/shared/components/LanguageToggle.tsx"
    - "frontend/src/shared/components/ui/dialog.tsx"
    - "frontend/public/locales/vi/auth.json"
    - "frontend/public/locales/en/auth.json"

key-decisions:
  - "Dialog component extended with hideCloseButton prop for non-dismissible session warning modal"
  - "Session timeout: 30 minutes inactivity with 2-minute warning threshold, activity throttled to 30s intervals"
  - "Token refresh scheduled at 80% of access token lifetime using setTimeout"
  - "Admin sidebar conditionally rendered based on Auth.Manage or Auth.View permission"
  - "UserFormDialog uses separate React Hook Form instances for create vs edit mode to avoid union type issues"

patterns-established:
  - "Auth API pattern: TanStack Query mutations wrapping openapi-fetch calls with as never casts for untyped client"
  - "Feature hook pattern: useUsers/useRoles encapsulate query + mutation state + dialog management"
  - "Permission matrix pattern: checkbox grid with module rows, action columns, Select All per row and column"
  - "Form validation: Zod schema with error codes mapped to i18n translation keys"
  - "Session management: useSession tracks activity events, shows warning modal before timeout"

requirements-completed: [AUTH-03, AUTH-04, UI-02]

# Metrics
duration: 9min
completed: 2026-02-28
---

# Phase 1 Plan 05: Auth UI Summary

**Split-layout login page with JWT auth, session timeout warning modal, user CRUD admin page, and permission matrix (10 modules x 6 actions) for role-based access control**

## Performance

- **Duration:** 9 min
- **Started:** 2026-02-28T14:07:03Z
- **Completed:** 2026-02-28T14:16:43Z
- **Tasks:** 2
- **Files modified:** 36

## Accomplishments
- Complete login flow: split-layout page with branding, form validation (React Hook Form + Zod), JWT authentication with auto token refresh
- Session management: 30-minute inactivity timeout with 2-minute countdown warning modal, extend or logout options
- User management admin page: sortable table, create/edit dialog with multi-role assignment
- Role management with permission matrix: 10 permission modules x 6 actions, Select All per row/column, change tracking
- Language preference persisted to backend on toggle (fire-and-forget PUT call)
- Admin sidebar section conditionally shown based on user permissions (Auth.Manage/Auth.View)

## Task Commits

Each task was committed atomically:

1. **Task 1: Login page, auth hooks, and session management with timeout warning** - `2f834e1` (feat)
2. **Task 2: User management and role/permission administration pages** - `ae7c211` (feat)

## Files Created/Modified

- `frontend/src/features/auth/api/auth-api.ts` - TanStack Query hooks for login, refresh, logout, language, getMe
- `frontend/src/features/auth/hooks/useAuth.ts` - Auth hook with login/logout/refresh and auto token scheduling
- `frontend/src/features/auth/hooks/useSession.ts` - Session timeout tracking with activity events and warning state
- `frontend/src/features/auth/components/LoginForm.tsx` - React Hook Form with Zod, show/hide password, remember me
- `frontend/src/features/auth/components/LoginPage.tsx` - Split layout: branding left, centered form right
- `frontend/src/features/auth/components/SessionWarningModal.tsx` - Non-dismissible countdown dialog
- `frontend/src/features/admin/api/admin-api.ts` - TanStack Query hooks for users, roles, permissions CRUD
- `frontend/src/features/admin/hooks/useUsers.ts` - User management state with dialog management
- `frontend/src/features/admin/hooks/useRoles.ts` - Role management state with permission grouping
- `frontend/src/features/admin/components/UserManagementPage.tsx` - Page with header, add button, table
- `frontend/src/features/admin/components/UserTable.tsx` - TanStack Table with sortable columns
- `frontend/src/features/admin/components/UserFormDialog.tsx` - Create/edit form with multi-role selection
- `frontend/src/features/admin/components/RoleManagementPage.tsx` - Page with role table and permission matrix
- `frontend/src/features/admin/components/RoleTable.tsx` - Roles list with click-to-select
- `frontend/src/features/admin/components/PermissionMatrix.tsx` - Checkbox grid with module grouping and Select All
- `frontend/src/app/routes/_authenticated/admin/users.tsx` - User management route
- `frontend/src/app/routes/_authenticated/admin/roles.tsx` - Role management route
- `frontend/src/app/routes/login.tsx` - Updated with LoginPage component and auth redirect
- `frontend/src/shared/components/AppShell.tsx` - Added SessionWarningModal integration
- `frontend/src/shared/components/AppSidebar.tsx` - Permission-gated admin navigation
- `frontend/src/shared/components/TopBar.tsx` - Uses useAuth logout
- `frontend/src/shared/components/LanguageToggle.tsx` - Backend language persistence
- `frontend/src/shared/components/ui/dialog.tsx` - Added hideCloseButton prop
- `frontend/public/locales/vi/auth.json` - Admin and permission module translations
- `frontend/public/locales/en/auth.json` - Admin and permission module translations

## Decisions Made

1. **hideCloseButton prop on Dialog** - Extended shadcn/ui Dialog component to support hiding the close button, needed for the non-dismissible session warning modal. This is a minimal, backward-compatible change.

2. **Separate form instances for create/edit** - UserFormDialog uses two separate React Hook Form instances (one for create, one for edit) rather than a union type, avoiding TypeScript issues with incompatible form schemas.

3. **Activity throttle at 30 seconds** - Session timeout activity listener is throttled to only reset timers every 30 seconds to avoid excessive timer restarts from rapid mouse movements.

4. **Permission-gated sidebar** - Admin section in sidebar is conditionally rendered by checking if the user's permissions array contains any permission starting with "Auth.Manage" or "Auth.View". This prevents non-admin users from seeing admin navigation.

5. **Fire-and-forget language sync** - Language toggle immediately updates i18n (instant UI), then fires a background mutation to persist preference to backend without blocking.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed TypeScript union type error in UserFormDialog toggleRole**
- **Found during:** Task 2
- **Issue:** Using a conditional `const form = isEditMode ? editForm : createForm` created a union type that TypeScript couldn't narrow for `getValues("roleIds")` and `setValue("roleIds", ...)`
- **Fix:** Split toggleRole into separate if/else branches that use editForm or createForm directly
- **Files modified:** frontend/src/features/admin/components/UserFormDialog.tsx
- **Verification:** `npm run build` succeeds
- **Committed in:** ae7c211

**2. [Rule 2 - Missing Critical] Extended Dialog component with hideCloseButton**
- **Found during:** Task 1
- **Issue:** SessionWarningModal requires a non-dismissible dialog without a close button, but shadcn/ui Dialog always renders the close X button
- **Fix:** Added optional `hideCloseButton` prop to DialogContent that conditionally renders the close button
- **Files modified:** frontend/src/shared/components/ui/dialog.tsx
- **Verification:** Build succeeds, dialog renders correctly with and without close button
- **Committed in:** 2f834e1

---

**Total deviations:** 2 auto-fixed (1 bug, 1 missing critical)
**Impact on plan:** Both fixes necessary for correct TypeScript compilation and proper session warning UX. No scope creep.

## Issues Encountered

- Pre-existing uncommitted files from other plan executions (audit-logs route, audit translations, architecture tests) were in the working directory. These were included in commits where they overlapped with build-generated files (routeTree.gen.ts) but otherwise left uncommitted for their respective plan to handle.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Auth UI complete: login, session management, user admin, role admin all functional
- Plan 01-06 (Audit UI) can build the audit log viewer using the established patterns (feature-based API layer, TanStack Table, i18n)
- Plan 01-07 (E2E verification) can test the complete login -> dashboard -> admin flow
- All future frontend features inherit the auth hook, session management, and permission-gated navigation patterns

## Self-Check: PASSED
