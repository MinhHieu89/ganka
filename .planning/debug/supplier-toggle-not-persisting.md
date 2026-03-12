---
status: diagnosed
trigger: "Toggling a supplier's active/inactive status shows success toast but doesn't persist"
created: 2026-03-11T00:00:00Z
updated: 2026-03-11T00:00:00Z
---

## Current Focus

hypothesis: No toggle/activate/deactivate endpoint exists - backend UpdateSupplierCommand ignores isActive, frontend sends isActive but backend never reads it
test: Trace full data flow from frontend toggle -> API call -> backend handler -> entity method
expecting: Missing link between frontend isActive field and backend persistence
next_action: Document root cause and required fixes

## Symptoms

expected: Toggling supplier active/inactive should persist the change
actual: Success toast shows but status reverts on refresh
errors: None (API returns success)
reproduction: Toggle any supplier's active status, refresh page, status unchanged
started: Always broken - feature was never implemented

## Eliminated

(none needed - root cause found on first hypothesis)

## Evidence

- timestamp: 2026-03-11
  checked: Supplier.cs domain entity
  found: Entity has Activate() and Deactivate() methods that set IsActive and call SetUpdatedAt()
  implication: Domain layer supports toggling

- timestamp: 2026-03-11
  checked: UpdateSupplierCommand and UpdateSupplierHandler
  found: Command has fields (Id, Name, ContactInfo, Phone, Email) - NO IsActive field. Handler calls supplier.Update(name, contactInfo, phone, email) which also does NOT touch IsActive.
  implication: Backend update endpoint completely ignores active status

- timestamp: 2026-03-11
  checked: PharmacyApiEndpoints.cs MapSupplierEndpoints
  found: Only 3 supplier endpoints: GET /suppliers, POST /suppliers, PUT /suppliers/{id}. NO dedicated toggle/activate/deactivate endpoint exists.
  implication: There is no API endpoint to change active status

- timestamp: 2026-03-11
  checked: Frontend UpdateSupplierInput type in pharmacy-api.ts
  found: Has optional isActive field, but updateSupplier() sends it via PUT to /api/pharmacy/suppliers/{id} which maps to UpdateSupplierCommand that has no IsActive property
  implication: Frontend sends isActive but backend silently ignores it

- timestamp: 2026-03-11
  checked: Frontend SupplierForm.tsx
  found: Form only has name and contactInfo fields. No toggle UI in the form itself. Toggle must be elsewhere or inline.
  implication: Toggle likely happens via useUpdateSupplier mutation with isActive in payload

## Resolution

root_cause: |
  THREE-LAYER GAP:
  1. Backend UpdateSupplierCommand record has no IsActive property - it only accepts (Id, Name, ContactInfo, Phone, Email)
  2. Backend UpdateSupplierHandler calls supplier.Update() which only updates name/contactInfo/phone/email - never calls Activate()/Deactivate()
  3. No dedicated toggle endpoint exists (e.g., PATCH /suppliers/{id}/toggle-active)

  The frontend sends isActive in the PUT payload, the backend deserializes it into UpdateSupplierCommand which has no IsActive field, so the value is silently dropped. The API returns 200 OK because the command itself succeeds (it updates name/contactInfo), hence the success toast.

fix: empty
verification: empty
files_changed: []
