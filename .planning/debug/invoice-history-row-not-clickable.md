---
status: diagnosed
trigger: "Invoice History page rows do not navigate to invoice detail on click"
created: 2026-03-17T00:00:00Z
updated: 2026-03-17T00:00:00Z
---

## Current Focus

hypothesis: The TableRow has cursor-pointer styling but no click handler or Link wrapper — only the invoice number cell contains a Link, so clicking anywhere else on the row does nothing.
test: Read InvoiceHistoryPage.tsx — confirmed
expecting: TableRow wrapped in Link, or onRowClick handler; neither exists
next_action: Report diagnosis to user

## Symptoms

expected: Clicking any part of a table row navigates to /billing/invoices/{id}
actual: Only clicking the invoice number text (which is a Link) navigates; clicking anywhere else on the row does nothing
errors: none (no console error — silent navigation failure)
reproduction: Go to /billing/invoices, click on a row cell that is NOT the invoice number text
started: Always — row-level navigation was never implemented

## Eliminated

- hypothesis: onRowClick handler exists but is wired to the wrong route
  evidence: No onRowClick handler exists anywhere in InvoiceHistoryPage.tsx
  timestamp: 2026-03-17T00:00:00Z

- hypothesis: DataTable component used (like shifts page) might suppress row clicks
  evidence: InvoiceHistoryPage does NOT use DataTable — it uses raw Table/TableBody/TableRow primitives
  timestamp: 2026-03-17T00:00:00Z

## Evidence

- timestamp: 2026-03-17T00:00:00Z
  checked: InvoiceHistoryPage.tsx line 118
  found: TableRow has className="cursor-pointer hover:bg-muted/50" but no onClick handler and is not wrapped in a Link
  implication: The pointer cursor is cosmetic only — there is no navigation wired to the row

- timestamp: 2026-03-17T00:00:00Z
  checked: InvoiceHistoryPage.tsx lines 119-126
  found: Only the first cell (invoice number) has a Link to="/billing/invoices/$invoiceId" — the other 6 cells are plain TableCell elements
  implication: Clicking patientName, status, amounts, or date cells does nothing

- timestamp: 2026-03-17T00:00:00Z
  checked: shifts.tsx line 144
  found: Shifts table uses DataTable with onRowClick={(_row, tanstackRow) => tanstackRow.toggleExpanded()} — a working row-click pattern
  implication: The project already has a proven pattern for row-level interaction via onRowClick prop

## Resolution

root_cause: InvoiceHistoryPage.tsx uses raw TableRow elements that have cursor-pointer styling but no navigation handler. The invoice number cell contains a Link, but all other cells are plain text, so only clicking the invoice number text triggers navigation. The entire row should navigate.

fix: Wrap the TableRow (or add an onClick) so clicking anywhere on the row navigates to /billing/invoices/$invoiceId. The cleanest approach for a raw Table setup is to add an onClick to the TableRow:
  onChange the TableRow to:
    <TableRow
      key={invoice.id}
      className="cursor-pointer hover:bg-muted/50"
      onClick={() => navigate({ to: "/billing/invoices/$invoiceId", params: { invoiceId: invoice.id } })}
    >
  and remove the Link wrapper around the invoice number (replace it with a plain <span> or keep it for accessibility but add e.stopPropagation() considerations).

verification: N/A — diagnosis only, no fix applied per task instructions
files_changed: []
