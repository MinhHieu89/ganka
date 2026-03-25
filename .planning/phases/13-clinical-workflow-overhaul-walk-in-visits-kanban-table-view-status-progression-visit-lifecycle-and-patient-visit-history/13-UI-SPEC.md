---
phase: 13
slug: clinical-workflow-overhaul
status: draft
shadcn_initialized: true
preset: stone (default style)
created: 2026-03-25
---

# Phase 13 -- UI Design Contract

> Visual and interaction contract for the clinical workflow overhaul: 8-column kanban, table view, bidirectional stage transitions, visit lifecycle, and patient visit history tab.

---

## Design System

| Property | Value |
|----------|-------|
| Tool | shadcn |
| Preset | default style, stone base color, CSS variables, radius 0.625rem |
| Component library | radix (via shadcn) |
| Icon library | @tabler/icons-react |
| Font | Be Vietnam Pro (sans), JetBrains Mono (mono) |

Source: `frontend/components.json`, `frontend/src/styles/globals.css`

---

## Spacing Scale

Declared values (must be multiples of 4):

| Token | Value | Usage |
|-------|-------|-------|
| xs | 4px | Icon gaps, inline badge padding |
| sm | 8px | Card internal padding (compact), kanban card gap |
| md | 16px | Default element spacing, section padding |
| lg | 24px | Column header padding, section gaps |
| xl | 32px | Layout gaps between major areas |
| 2xl | 48px | Page-level vertical spacing |
| 3xl | 64px | Not used in this phase |

Exceptions:
- Kanban column width: 200px (each of 9 columns including Done; narrower than current 220px to fit more columns before scroll)
- Timeline panel width: 300px fixed (D-14 from CONTEXT.md)
- Touch target minimum: 44px for drag handles and advance buttons

---

## Typography

| Role | Size | Weight | Line Height |
|------|------|--------|-------------|
| Body | 14px | 400 (regular) | 1.5 |
| Label / Caption | 12px | 500 (medium) | 1.4 |
| Heading (section) | 16px | 600 (semibold) | 1.3 |
| Display (page title) | 20px | 600 (semibold) | 1.2 |

Source: Matches existing codebase patterns (`text-sm` = 14px body, `text-xs` = 12px labels, `font-semibold` headings).

---

## Color

| Role | Value | Usage |
|------|-------|-------|
| Dominant (60%) | `--background` oklch(1 0 0) | Page background, kanban board background |
| Secondary (30%) | `--card` oklch(1 0 0) / `--secondary` oklch(0.97 0 0) | Kanban column cards, table rows, detail panels |
| Accent (10%) | `--primary` oklch(0.205 0 0) | Primary CTA buttons, active tab indicator, drag-over ring |
| Destructive | `--destructive` oklch(0.577 0.245 27.325) | Stage reversal warning, cancelled visit badge |

Accent reserved for:
- "New Visit" primary CTA button
- Active tab indicator on patient profile tabs
- Drag-over ring highlight on kanban columns (existing: `ring-primary/30`)
- Toggle button active state (kanban/table view switch)
- Stage advance button (primary variant)

### Kanban Column Accent Colors (per-stage top border)

Each of the 9 columns (8 workflow stages + Done) gets a unique `border-t-2` accent color:

| Column | Stage | Accent Class |
|--------|-------|-------------|
| Reception | 0 | `border-t-stone-400` |
| Refraction/VA | 1 | `border-t-blue-500` |
| Doctor Exam | 2 | `border-t-emerald-500` |
| Diagnostics | 3 | `border-t-cyan-500` |
| Doctor Reads | 4 | `border-t-teal-500` |
| Rx | 5 | `border-t-amber-500` |
| Cashier | 6 | `border-t-orange-500` |
| Pharmacy/Optical | 7 | `border-t-violet-500` |
| Done | completed | `border-t-muted-foreground/20` |

### Visit Status Badge Colors

| Status | Badge Variant | Meaning |
|--------|--------------|---------|
| Draft | `outline` | Visit in progress |
| Signed | `default` (primary bg) | Doctor has signed off |
| Amended | `secondary` | Corrections applied after sign-off |
| Cancelled | `destructive` | Visit cancelled |

Source: Existing `STATUS_MAP` in `VisitDetailPage.tsx`.

---

## Component Inventory

### New Components

| Component | Description | shadcn Primitives Used |
|-----------|-------------|----------------------|
| `WorkflowToolbar` | Header bar with title, patient count badge, view toggle (kanban/table), filter controls, and "New Visit" button | Button, Badge, ToggleGroup |
| `ViewToggle` | Icon toggle between kanban grid and table list views. Persists to localStorage | ToggleGroup (with IconLayoutKanban, IconTable icons) |
| `WorkflowTableView` | Full-width sortable/filterable table showing all active visits | Table, Badge, Button |
| `StageReversalDialog` | Modal for reversing a visit to a previous stage. Requires mandatory reason text | Dialog, Textarea, Button, Select |
| `VisitHistoryTab` | 2-column layout: timeline left (300px), detail panel right (remaining) | ScrollArea, Card, Badge |
| `VisitTimeline` | Vertical timeline of past visits with compact summary cards | Card, Badge, ScrollArea |
| `VisitTimelineCard` | Individual timeline entry: date, doctor, primary diagnosis, status badge | Card, Badge |
| `VisitHistoryDetail` | Read-only detail panel reusing existing visit sections | Card, all existing section components |

### Modified Components

| Component | Changes |
|-----------|---------|
| `WorkflowDashboard` | Expand from 5 to 9 columns (8 stages + Done). Add toolbar with view toggle. Add table view conditional rendering |
| `KanbanColumn` | Update column width from 220px to 200px. Add 5 new accent color entries |
| `PatientCard` | Add patient name as clickable link to patient profile (D-17). Add backward arrow button for eligible stages |
| `SignOffSection` | No visual changes -- auto-advance is backend-triggered (D-11) |
| `PatientProfilePage` | Add "Visit History" tab trigger (D-13) |
| `OpticalPrescriptionSection` | Auto-expand when data exists (D-19, `defaultOpen` prop) |
| `OsdiSection` / `VisitDetailPage` | Show OSDI questionnaire answers inline (D-18). Realtime score update via React Query invalidation (D-20) |

### Existing Components Reused (no changes)

- `RefractionSection` -- in history detail panel (read-only)
- `DryEyeSection` -- in history detail panel (read-only)
- `DiagnosisSection` -- in history detail panel (read-only)
- `DrugPrescriptionSection` -- in history detail panel (read-only)
- `OsdiAnswersSection` -- in history detail panel (read-only)
- `NewVisitDialog` -- unchanged walk-in creation dialog

---

## Interaction Contracts

### Kanban Board (8+1 columns)

- **Horizontal scroll**: `overflow-x-auto` on the kanban container. All 9 columns rendered at 200px width. Scroll to see off-screen columns (D-03).
- **Drag and drop**: Existing @dnd-kit/core pattern. Drag a PatientCard between columns to advance or (where allowed) reverse stage.
- **Forward drag**: Any column to any later column (stage skipping allowed per D-12).
- **Backward drag**: Only to allowed previous stages. If drag target is not an allowed backward transition, drop is rejected (card snaps back).
- **Backward transition**: Opens StageReversalDialog with mandatory reason textarea before confirming. Cancel returns card to original column.
- **Done column**: Shows visits completed today (filtered by visitDate = today). Cards in Done are not draggable.
- **Column header**: Stage name (translated) + visit count badge.

### View Toggle (Kanban / Table)

- **Location**: Right side of WorkflowToolbar, before "New Visit" button.
- **Icons**: `IconLayoutKanban` for kanban view, `IconTable` for table view.
- **Persistence**: `localStorage` key `ganka:workflow-view-mode`, values `"kanban"` | `"table"`.
- **Default**: `"kanban"` if no stored preference.
- **Transition**: Instant swap, no animation. Both views share the same data source (useActiveVisits).

### Table View

- **Columns**: Patient Name (linked), Doctor, Current Stage, Wait Time, Visit Start Time, Status, Actions.
- **Sorting**: Click column header to sort. Default sort: Wait Time descending (longest wait first).
- **Filtering**: Stage dropdown filter in toolbar (multi-select). Doctor dropdown filter.
- **Row click**: Navigates to `/visits/$visitId`.
- **Actions column**: Forward arrow button (advance to next stage). Backward arrow button (if reversal allowed, opens StageReversalDialog).
- **Empty state**: Shown when no active visits match current filters.

### Stage Reversal Dialog

- **Trigger**: Backward drag-and-drop on kanban, or backward arrow button in table view.
- **Content**: Current stage label, target stage label, mandatory reason textarea (minimum 10 characters).
- **Actions**: "Reverse Stage" (primary, disabled until reason meets minimum length) and "Cancel" (outline).
- **On confirm**: Calls backend ReverseWorkflowStage endpoint. Shows toast on success. Closes dialog.

### Patient Visit History Tab (D-13, D-14, D-15, D-16)

- **Tab trigger**: "Visit History" added after existing "Treatments" tab in PatientProfilePage.
- **Layout**: 2-column. Left: VisitTimeline at 300px fixed width with vertical scroll. Right: VisitHistoryDetail taking remaining width.
- **Timeline cards**: Compact vertical stack. Each card shows: visit date (formatted), doctor name, primary diagnosis text (truncated to 1 line), status badge (Draft/Signed/Amended).
- **Selection**: Click a timeline card to load its detail in the right panel. Selected card gets `ring-2 ring-primary` highlight.
- **Default selection**: Most recent visit auto-selected on tab mount.
- **Detail panel**: Read-only rendering of the same sections from VisitDetailPage: refraction, dry eye, OSDI answers, diagnosis, drug prescription, optical prescription, images, notes, sign-off info.
- **Empty state**: Shown when patient has no visit history.
- **Scroll**: Timeline scrolls independently from detail panel (`overflow-y-auto` on each).

### Patient Name Link (D-17)

- **Kanban card**: Patient name text becomes a `<Link>` to `/patients/$patientId`. Styled as `text-primary hover:underline cursor-pointer`. Click navigates; does not interfere with drag (drag requires 8px movement threshold).
- **Visit detail page**: Patient name in header becomes same link pattern.

### OSDI Answers Display (D-18)

- **Location**: Within the OSDI section on visit detail page, below the score display.
- **Format**: Collapsible section showing individual question answers. Uses existing `OsdiAnswersSection` component.
- **Default state**: Collapsed. Expand via chevron toggle.

### Optical Prescription Auto-expand (D-19)

- **Behavior**: If optical prescription data exists on the visit, the collapsible section renders with `defaultOpen={true}`.
- **No data**: Section remains collapsed (unchanged behavior).

### Realtime OSDI Update (D-20)

- **Mechanism**: When OSDI questionnaire is completed (saved), React Query cache for the visit is invalidated, causing the visit detail page to refetch and display updated scores.
- **Visual feedback**: Score value updates in place. No separate loading indicator needed (data is small).

---

## Copywriting Contract

All copy must be provided in both English and Vietnamese (UI-01, UI-02). English values listed below; Vietnamese translations to be added to i18n files.

| Element | English Copy | i18n Key |
|---------|-------------|----------|
| Primary CTA | "New Visit" | `workflow.newVisit` (existing) |
| View toggle kanban label | "Board" | `workflow.viewBoard` |
| View toggle table label | "Table" | `workflow.viewTable` |
| Empty state heading (kanban) | "No active visits" | `workflow.emptyState.title` |
| Empty state body (kanban) | "Create a new visit to get started" | `workflow.emptyState.description` |
| Empty state heading (table) | "No visits match your filters" | `workflow.table.emptyState.title` |
| Empty state body (table) | "Try adjusting your filters or create a new visit" | `workflow.table.emptyState.description` |
| Stage reversal dialog title | "Reverse Workflow Stage" | `workflow.reversal.title` |
| Stage reversal reason label | "Reason for reversal" | `workflow.reversal.reasonLabel` |
| Stage reversal reason placeholder | "Explain why this visit needs to return to a previous stage..." | `workflow.reversal.reasonPlaceholder` |
| Stage reversal confirm button | "Reverse Stage" | `workflow.reversal.confirm` |
| Stage reversal success toast | "Visit moved back to {stage}" | `workflow.reversal.success` |
| Stage reversal error toast | "Failed to reverse stage. Please try again." | `workflow.reversal.error` |
| Visit history tab label | "Visit History" | `patient.visitHistory.tab` |
| Visit history empty heading | "No visit history" | `patient.visitHistory.empty.title` |
| Visit history empty body | "This patient has no recorded visits yet" | `patient.visitHistory.empty.description` |
| Done column label | "Done" | `workflow.done` (existing) |
| Table column: Patient | "Patient" | `workflow.table.patient` |
| Table column: Doctor | "Doctor" | `workflow.table.doctor` |
| Table column: Stage | "Stage" | `workflow.table.stage` |
| Table column: Wait Time | "Wait Time" | `workflow.table.waitTime` |
| Table column: Visit Time | "Visit Time" | `workflow.table.visitTime` |
| Table column: Status | "Status" | `workflow.table.status` |
| Table column: Actions | "Actions" | `workflow.table.actions` |

### Destructive Actions

| Action | Confirmation Approach |
|--------|----------------------|
| Stage reversal | StageReversalDialog with mandatory reason (minimum 10 chars). Not a destructive-styled button -- uses primary variant since reversal is a controlled clinical action, not data deletion. |

No other destructive actions in this phase. Visit cancellation is not in scope.

---

## Loading & Error States

| View | Loading State | Error State |
|------|--------------|-------------|
| Kanban board | 9 skeleton columns with 2-3 skeleton cards each (existing pattern) |  "Failed to load visits. Please refresh the page." with retry button |
| Table view | Table skeleton with 5 shimmer rows | Same error as kanban |
| Visit history timeline | 3 skeleton timeline cards | "Failed to load visit history" with retry button |
| Visit history detail | Section skeletons matching existing visit detail page | "Failed to load visit details" inline |
| Stage advance/reverse | Button shows spinner, disabled during mutation | Toast with error message |

---

## Registry Safety

| Registry | Blocks Used | Safety Gate |
|----------|-------------|-------------|
| shadcn official | Table, Tabs, TabsList, TabsTrigger, TabsContent, Badge, Button, Card, Dialog, Textarea, Select, ToggleGroup, ScrollArea, Skeleton, Tooltip | not required |
| Third-party | none | not applicable |

---

## Responsive Behavior

This phase targets desktop use (clinic workstations). No mobile-responsive requirements (UIX-01 is v2).

- **Kanban**: Minimum viewport width 1024px. Horizontal scroll for columns beyond viewport.
- **Table**: Full width, horizontal scroll if columns exceed viewport.
- **Visit history**: 2-column layout collapses to stacked layout below 768px (timeline on top, detail below) as a graceful degradation only.

---

## Accessibility

- Kanban drag-and-drop: Provide keyboard alternative via advance/reverse buttons on each card (not drag-only).
- Table view: Standard semantic `<table>` with sortable column headers using `aria-sort`.
- Stage reversal dialog: Focus trapped. Escape to close. Autofocus on reason textarea.
- Timeline: Arrow key navigation between timeline cards. `aria-selected` on active card.
- All icons paired with text labels or `aria-label` attributes.
- Color is never the sole indicator: stage names always accompany accent colors. Status badges include text labels.

---

## Checker Sign-Off

- [ ] Dimension 1 Copywriting: PASS
- [ ] Dimension 2 Visuals: PASS
- [ ] Dimension 3 Color: PASS
- [ ] Dimension 4 Typography: PASS
- [ ] Dimension 5 Spacing: PASS
- [ ] Dimension 6 Registry Safety: PASS

**Approval:** pending
