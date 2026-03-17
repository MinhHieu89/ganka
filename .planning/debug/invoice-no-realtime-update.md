---
status: investigating
trigger: "When a doctor adds a drug prescription to a visit, billing line items are created on the backend (via DrugPrescriptionAdded event), but the cashier viewing the invoice detail page does NOT see the new line items until they manually refresh the browser."
created: 2026-03-17T00:00:00Z
updated: 2026-03-17T00:00:00Z
symptoms_prefilled: true
goal: find_root_cause_only
---

## Current Focus

hypothesis: Invoice detail page has no real-time update mechanism (no polling, no SignalR subscription, no query invalidation from events)
test: Read invoice detail page component, check for SignalR usage, check DrugPrescriptionAdded handler for event emission
expecting: No real-time notification path exists from backend event to frontend cache invalidation
next_action: Read invoice detail page component

## Symptoms

expected: Cashier sees new drug prescription line items immediately after doctor adds them
actual: Cashier must manually refresh the browser to see new line items
errors: none (no error, just stale data)
reproduction: Doctor adds drug prescription on visit -> backend fires DrugPrescriptionAdded event -> billing line items created -> cashier on invoice detail page sees no update
started: unknown, likely since feature was shipped

## Eliminated

## Evidence

## Resolution

root_cause:
fix: N/A (diagnose only)
verification:
files_changed: []
