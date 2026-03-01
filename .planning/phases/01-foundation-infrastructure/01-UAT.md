---
status: complete
phase: 01-foundation-infrastructure
source: 01-01-SUMMARY.md, 01-02-SUMMARY.md, 01-03-SUMMARY.md, 01-04-SUMMARY.md, 01-05-SUMMARY.md, 01-06-SUMMARY.md, 01-07-SUMMARY.md
started: 2026-03-01T12:00:00Z
updated: 2026-03-01T06:50:00Z
---

## Current Test

[testing complete]

## Tests

### 1. Backend Solution Builds
expected: Run `dotnet build backend/Ganka28.slnx` — compiles all 43 projects with 0 errors.
result: pass

### 2. Frontend Builds
expected: Run `cd frontend && npm run build` — compiles TanStack Start SPA with 0 errors.
result: pass

### 3. All Unit and Architecture Tests Pass
expected: Run `dotnet test backend/Ganka28.slnx` — all tests pass (Auth.Unit, Audit.Unit, ArchitectureTests). 0 failures.
result: pass

### 4. Backend Starts and Swagger Loads
expected: Run the backend (`dotnet run --project backend/src/Bootstrapper`). Navigate to Swagger UI — Auth endpoints (login, refresh, logout, users, roles, permissions) and Audit endpoints (audit-logs, access-logs) are listed.
result: pass

### 5. Frontend Renders Login Page
expected: Run frontend dev server (`cd frontend && npm run dev`). Navigate to localhost — see split-layout login page with branding on left and login form on right. Form has email, password, and "Remember me" checkbox.
result: pass

### 6. Login with Invalid Credentials Shows Error
expected: Enter wrong email/password on the login page. An inline error message appears (not a blank response). The page does not redirect.
result: pass

### 7. Login with Valid Credentials Redirects to Dashboard
expected: Log in with admin@ganka28.com / Admin@123456. Redirected to dashboard. App shell appears with collapsible sidebar and top bar showing user info.
result: pass

### 8. Language Toggle Switches All UI Text
expected: Click the language toggle button (VI/EN) in the top bar. All sidebar labels, page titles, and button text switch between Vietnamese and English instantly.
result: pass

### 9. Sidebar Collapses to Icon-Only Mode
expected: Click the sidebar collapse button. Sidebar shrinks to icon-only mode. Click again to expand. Navigation items remain functional in both states.
result: pass

### 10. User Management Page Lists All Users
expected: Navigate to Admin > Users. A table shows all seeded users (at least the root admin). Columns are sortable. "Add User" button is visible.
result: pass

### 11. Create User Dialog Works
expected: Click "Add User". A dialog opens with fields for name, email, password, and multi-role assignment (8 roles available as checkboxes). Fill the form and submit — new user appears in the table.
result: pass

### 12. Role Management Page Shows 8 System Roles
expected: Navigate to Admin > Roles. A table lists 8 system roles (Admin, Doctor, Technician, Nurse, Cashier, OpticalStaff, Manager, Accountant). Clicking a role shows the permission matrix.
result: pass

### 13. Permission Matrix Displays 10 Modules x 6 Actions
expected: Select a role on the Role Management page. Permission matrix shows a checkbox grid: 10 module rows (Auth, Audit, Patient, Clinical, Scheduling, Pharmacy, Optical, Billing, Treatment, Reporting) x 6 action columns (View, Create, Update, Delete, Manage, Export). Select All per row/column works.
result: pass

### 14. Audit Log Page Shows Entries with Filters
expected: Navigate to Admin > Audit Logs. Table shows audit entries with timestamps, user, action type, and entity. Filter controls (user, action type, date range) are present and functional.
result: pass

### 15. Audit Log Expandable Row Details
expected: Click on an audit log row to expand it. Detail view shows field-level changes with old values (red/strikethrough) and new values (green).
result: pass

### 16. Audit Log CSV Export
expected: Click the "Export" button on the Audit Log page. A CSV file downloads containing the filtered audit records.
result: pass

### 17. Auth Guard Redirects Unauthenticated Users
expected: Log out (or open a new incognito window). Try navigating directly to /dashboard or /admin/users. Redirected back to the login page.
result: pass

## Summary

total: 17
passed: 17
issues: 0
pending: 0
skipped: 0

## Gaps

[none yet]
