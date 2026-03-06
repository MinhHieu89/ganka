---
status: diagnosed
trigger: "Finalize invoice endpoint allows finalizing invoices with zero balance and no line items"
created: 2026-03-06T00:00:00Z
updated: 2026-03-06T00:00:00Z
---

## Current Focus

hypothesis: Invoice.Finalize() only checks IsFullyPaid (BalanceDue <= 0) but never checks that the invoice has line items or a positive total. A brand-new empty invoice has TotalAmount=0, PaidAmount=0, so BalanceDue=0 and IsFullyPaid=true, passing the only guard.
test: Trace the Finalize method logic with zero-value invoice
expecting: Confirm no line-item or positive-total validation exists
next_action: Report root cause

## Symptoms

expected: Finalize endpoint should reject invoices that have no line items or zero total amount
actual: Empty invoices (0 line items, 0 balance) can be finalized successfully
errors: No error thrown - that IS the bug (missing validation)
reproduction: Create invoice -> immediately call finalize (no line items, no payments needed)
started: Always broken - validation was never implemented

## Eliminated

(none - root cause found on first hypothesis)

## Evidence

- timestamp: 2026-03-06T00:00:00Z
  checked: Invoice.Finalize() domain method (Invoice.cs lines 200-215)
  found: Only two guards exist - EnsureDraft() (line 202) and !IsFullyPaid (line 204). No check for _lineItems.Count > 0 or TotalAmount > 0.
  implication: The domain entity itself lacks the necessary business rule enforcement.

- timestamp: 2026-03-06T00:00:00Z
  checked: IsFullyPaid and BalanceDue computed properties (Invoice.cs lines 59-62)
  found: BalanceDue = TotalAmount - PaidAmount. For empty invoice: 0 - 0 = 0. IsFullyPaid = BalanceDue <= 0, so 0 <= 0 = true.
  implication: An empty invoice trivially satisfies IsFullyPaid because 0 <= 0 is true.

- timestamp: 2026-03-06T00:00:00Z
  checked: FinalizeInvoiceHandler (FinalizeInvoice.cs lines 20-46)
  found: Handler only checks invoice != null, then delegates to invoice.Finalize(). No additional validation at application layer.
  implication: Neither domain nor application layer prevents empty invoice finalization.

- timestamp: 2026-03-06T00:00:00Z
  checked: Existing unit tests (InvoiceCrudHandlerTests.cs lines 163-213)
  found: Two finalize tests exist - one for success (fully paid) and one for outstanding balance. No test for empty invoice scenario.
  implication: The gap was never caught because no test covers this edge case.

## Resolution

root_cause: The Invoice.Finalize() domain method (Invoice.cs line 200) only validates two conditions: (1) invoice is in Draft status via EnsureDraft(), and (2) invoice is fully paid via IsFullyPaid check. It does NOT validate that the invoice has at least one line item or a positive TotalAmount. Because BalanceDue is computed as TotalAmount - PaidAmount (0 - 0 = 0), and IsFullyPaid is BalanceDue <= 0 (0 <= 0 = true), an empty invoice with zero line items trivially passes both guards and gets finalized.

fix: (not yet applied)
verification: (not yet verified)
files_changed: []
