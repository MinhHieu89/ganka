---
phase: 01-foundation-infrastructure
plan: 02
subsystem: ui
tags: [tanstack-start, shadcn-ui, i18next, tailwind-v4, zustand, tanstack-router, tanstack-query, openapi-fetch, react]

# Dependency graph
requires:
  - phase: none
    provides: "First frontend plan, no prior frontend dependency"
provides:
  - "TanStack Start SPA scaffold with Vite plugin configuration"
  - "shadcn/ui component library (16 components) with Stone+Green theme"
  - "i18next bilingual support (Vietnamese default, English) with namespace-per-feature"
  - "App shell layout: collapsible sidebar + top bar + main content area"
  - "Auth guard layout route redirecting unauthenticated users to /login"
  - "Zustand stores for auth state (in-memory) and sidebar collapse (persisted)"
  - "openapi-fetch client ready for API integration"
  - "TanStack Query client configured"
  - "Translation files for common, auth, and audit namespaces"
affects: [01-05, 01-06, 02, 03, 04, 05, 06, 07, 08, 09]

# Tech tracking
tech-stack:
  added: ["@tanstack/react-start@1.163.3", "@tanstack/react-router@1.163.3", "@tanstack/react-query@5.x", "@tanstack/react-table@8.x", "shadcn/ui", "zustand@5.x", "i18next@24.x", "react-i18next@15.x", "openapi-fetch@0.13.x", "tailwindcss@4.x", "@tabler/icons-react@3.x", "react-hook-form@7.x", "zod@3.x", "sonner@1.x", "vite@7.x"]
  patterns: ["SPA mode via TanStack Start Vite plugin", "File-based routing with TanStack Router", "Zustand store pattern (in-memory vs persisted)", "i18next namespace-per-feature pattern", "shadcn/ui component customization with CSS variables", "Path alias @ -> src/"]

key-files:
  created:
    - "frontend/vite.config.ts"
    - "frontend/components.json"
    - "frontend/src/app/router.tsx"
    - "frontend/src/app/routes/__root.tsx"
    - "frontend/src/app/routes/_authenticated.tsx"
    - "frontend/src/app/routes/login.tsx"
    - "frontend/src/app/routes/_authenticated/dashboard.tsx"
    - "frontend/src/shared/components/AppShell.tsx"
    - "frontend/src/shared/components/AppSidebar.tsx"
    - "frontend/src/shared/components/TopBar.tsx"
    - "frontend/src/shared/components/LanguageToggle.tsx"
    - "frontend/src/shared/i18n/i18n.ts"
    - "frontend/src/shared/stores/authStore.ts"
    - "frontend/src/shared/stores/sidebarStore.ts"
    - "frontend/src/shared/lib/api-client.ts"
    - "frontend/src/styles/globals.css"
  modified: []

key-decisions:
  - "TanStack Start v1.163 uses pure Vite plugin (no vinxi) -- adapted from plan's vinxi-based setup"
  - "Router exports getRouter() instead of createRouter() per TanStack Start v1.163 API"
  - "i18n created in Task 1 (ahead of Task 2 plan) because __root.tsx imports it"
  - "Login page uses split layout (branding left, form right) per CONTEXT.md decision"
  - "Used @tabler/icons-react for all icons per shadcn/ui configuration"

patterns-established:
  - "Route structure: _authenticated layout wraps all protected routes"
  - "Auth check: useAuthStore.getState() in beforeLoad for auth guard"
  - "Translation: useTranslation hook with namespace parameter"
  - "Sidebar navigation: array-driven nav items with i18n keys"
  - "App shell: SidebarProvider > AppSidebar + SidebarInset > TopBar + Outlet"

requirements-completed: [UI-01, UI-02]

# Metrics
duration: 17min
completed: 2026-02-28
---

# Phase 1 Plan 2: Frontend Scaffolding Summary

**TanStack Start SPA with shadcn/ui (Stone+Green, no border radius), bilingual i18next (Vietnamese default), collapsible sidebar, auth guard routing, and Zustand stores**

## Performance

- **Duration:** 17 min
- **Started:** 2026-02-28T13:20:12Z
- **Completed:** 2026-02-28T13:37:40Z
- **Tasks:** 2
- **Files modified:** 55

## Accomplishments
- TanStack Start SPA fully scaffolded with Vite plugin, shadcn/ui (16 components), and Tailwind v4
- App shell layout with collapsible sidebar (icon-only mode), top bar with user menu and language toggle
- i18next bilingual support (Vietnamese default) with translation files for common, auth, and audit namespaces
- Auth guard on `/_authenticated` layout route redirects to `/login` when unauthenticated
- Zustand stores for auth state (in-memory, no localStorage for security) and sidebar collapse (persisted)
- openapi-fetch client and TanStack Query configured for future API integration

## Task Commits

Each task was committed atomically:

1. **Task 1: Scaffold TanStack Start SPA with all dependencies and shadcn/ui** - `5871dd3` (feat)
2. **Task 2: Build app shell layout, i18next, language toggle, and auth guard routing** - `2f671db` (feat)

## Files Created/Modified

- `frontend/package.json` - All frontend dependencies (TanStack Start, shadcn/ui, i18next, Zustand, etc.)
- `frontend/vite.config.ts` - Vite config with TanStack Start plugin, Tailwind, path aliases
- `frontend/components.json` - shadcn/ui configuration (Stone base, components alias)
- `frontend/tsconfig.json` - TypeScript config with @ path alias
- `frontend/src/styles/globals.css` - Tailwind v4 with Stone+Green theme, no border radius, Inter font
- `frontend/src/app/router.tsx` - TanStack Router with getRouter() export
- `frontend/src/app/routes/__root.tsx` - Root layout with QueryClientProvider, TooltipProvider, Suspense
- `frontend/src/app/routes/_authenticated.tsx` - Auth guard layout route
- `frontend/src/app/routes/_authenticated/dashboard.tsx` - Protected dashboard placeholder
- `frontend/src/app/routes/login.tsx` - Public login page with split layout
- `frontend/src/app/routes/index.tsx` - Root redirect to /dashboard
- `frontend/src/shared/components/AppShell.tsx` - Main layout combining sidebar + topbar + outlet
- `frontend/src/shared/components/AppSidebar.tsx` - Navigation sidebar with i18n labels
- `frontend/src/shared/components/TopBar.tsx` - Top bar with user menu, language toggle, logout
- `frontend/src/shared/components/LanguageToggle.tsx` - VI/EN language switch button
- `frontend/src/shared/i18n/i18n.ts` - i18next config with Vietnamese default, http-backend
- `frontend/src/shared/stores/authStore.ts` - Auth Zustand store (in-memory only)
- `frontend/src/shared/stores/sidebarStore.ts` - Sidebar collapse Zustand store (localStorage)
- `frontend/src/shared/lib/api-client.ts` - openapi-fetch client for VITE_API_URL
- `frontend/src/shared/lib/utils.ts` - cn() utility for Tailwind class merging
- `frontend/src/shared/hooks/use-mobile.ts` - Mobile breakpoint detection hook
- `frontend/src/shared/components/ui/*.tsx` - 16 shadcn/ui components
- `frontend/public/locales/vi/*.json` - Vietnamese translations (common, auth, audit)
- `frontend/public/locales/en/*.json` - English translations (common, auth, audit)

## Decisions Made

1. **TanStack Start v1.163 API change** - The installed version (1.163.3) uses a pure Vite plugin approach instead of the vinxi-based `defineConfig` from earlier versions. Adapted configuration accordingly with `tanstackStart()` Vite plugin and `getRouter()` export instead of `createRouter()`.

2. **i18n moved to Task 1** - Created i18n.ts during Task 1 because `__root.tsx` imports it and the build would fail without it. This is a minor sequence adjustment from the plan.

3. **Static SPA prerendering** - TanStack Start SPA mode prerenders a shell at `/_shell.html` for the root route. Added an index route that redirects to `/dashboard` to satisfy the prerender.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] TanStack Start API migration from vinxi to Vite plugin**
- **Found during:** Task 1
- **Issue:** Plan specified `@tanstack/react-start/config` with `defineConfig` and vinxi commands, but TanStack Start v1.163 removed vinxi dependency and uses `@tanstack/react-start/plugin/vite` with pure Vite
- **Fix:** Replaced `app.config.ts` with `vite.config.ts`, changed scripts from vinxi to vite commands, exported `getRouter()` instead of `createRouter()`
- **Files modified:** vite.config.ts, package.json, router.tsx
- **Verification:** `npm run build` succeeds
- **Committed in:** 5871dd3

**2. [Rule 3 - Blocking] Icon name correction for sidebar trigger**
- **Found during:** Task 2
- **Issue:** `IconPanelLeftClose` does not exist in @tabler/icons-react
- **Fix:** Changed to `IconLayoutSidebarLeftCollapse` which exists in the package
- **Files modified:** sidebar.tsx
- **Verification:** Build succeeds, icon renders
- **Committed in:** 2f671db

---

**Total deviations:** 2 auto-fixed (2 blocking)
**Impact on plan:** Both fixes necessary due to version differences between plan research and actual installed packages. No scope creep.

## Issues Encountered

- TanStack Start v1.163.3 has a significantly different configuration API from the RC versions documented in the research. The vinxi-based approach (`defineConfig`, `vinxi dev/build`) has been replaced with a native Vite plugin (`tanstackStart()`, `vite dev/build`). This required adapting the project structure but the end result is simpler (fewer dependencies, standard Vite tooling).

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Frontend scaffold complete, ready for feature UIs to plug into
- Auth UI (Plan 05) can build on the login page placeholder and auth store
- Audit UI (Plan 06) can use the app shell and audit translation namespace
- All future frontend plans inherit bilingual support automatically through i18n setup

## Self-Check: PASSED

All 18 key files verified present. Both task commits (5871dd3, 2f671db) found in git log. `npm run build` succeeds.

---
*Phase: 01-foundation-infrastructure*
*Completed: 2026-02-28*
