---
status: resolved
trigger: "ApplyDiscount and RequestRefund endpoints do not auto-populate RequestedById from ICurrentUser"
created: 2026-03-06T00:00:00Z
updated: 2026-03-06T18:00:00Z
---

## Current Focus

hypothesis: ApplyDiscount and RequestRefund handlers do not inject ICurrentUser and instead require RequestedById from the client
test: Compare handler signatures and command records against working handlers (CreateInvoice, RecordPayment, OpenShift, ProcessRefund)
expecting: Working handlers inject ICurrentUser and use currentUser.UserId; broken handlers do not
next_action: Return diagnosis

## Symptoms

expected: POST /api/billing/discounts and POST /api/billing/refunds should auto-populate RequestedById from the authenticated user's JWT claims (ICurrentUser.UserId), like other billing handlers do
actual: Both endpoints require the client to explicitly send RequestedById in the request body; omitting it causes FluentValidation error 'RequestedById must not be empty'
errors: Validation error - 'RequestedById must not be empty'
reproduction: POST to /api/billing/discounts or /api/billing/refunds without RequestedById in body
started: Since initial implementation

## Eliminated

(none needed - root cause identified on first hypothesis)

## Evidence

- timestamp: 2026-03-06T00:00:00Z
  checked: ApplyDiscount.cs handler signature (line 45-50)
  found: Handler does NOT inject ICurrentUser. Uses command.RequestedById directly (line 78).
  implication: No server-side enrichment of RequestedById

- timestamp: 2026-03-06T00:00:00Z
  checked: RequestRefund.cs handler signature (line 41-46)
  found: Handler does NOT inject ICurrentUser. Uses command.RequestedById directly (line 89).
  implication: No server-side enrichment of RequestedById

- timestamp: 2026-03-06T00:00:00Z
  checked: ApplyDiscountCommand record (line 14-20)
  found: RequestedById is a required Guid parameter in the command record, validated as NotEmpty (line 35)
  implication: Client must send it explicitly or validation fails

- timestamp: 2026-03-06T00:00:00Z
  checked: RequestRefundCommand record (line 14-19)
  found: RequestedById is a required Guid parameter in the command record, validated as NotEmpty (line 31)
  implication: Client must send it explicitly or validation fails

- timestamp: 2026-03-06T00:00:00Z
  checked: BillingApiEndpoints.cs discount endpoint (line 133-137)
  found: Command is passed straight through to bus.InvokeAsync without enrichment
  implication: No endpoint-level enrichment either

- timestamp: 2026-03-06T00:00:00Z
  checked: BillingApiEndpoints.cs refund endpoint (line 164-168)
  found: Command is passed straight through to bus.InvokeAsync without enrichment
  implication: No endpoint-level enrichment either

- timestamp: 2026-03-06T00:00:00Z
  checked: CreateInvoiceHandler (line 37-43) - WORKING PATTERN
  found: Injects ICurrentUser, uses currentUser.BranchId (line 62). Does NOT include user-specific IDs in the command record.
  implication: Established pattern is to inject ICurrentUser in handler and use it server-side

- timestamp: 2026-03-06T00:00:00Z
  checked: RecordPaymentHandler (line 65-73) - WORKING PATTERN
  found: Injects ICurrentUser, uses currentUser.UserId for payment.RecordedById (line 113) and currentUser.BranchId (line 101). Does NOT include user ID in command record.
  implication: Confirms pattern: user IDs come from ICurrentUser, not from client

- timestamp: 2026-03-06T00:00:00Z
  checked: OpenShiftHandler (line 33-39) - WORKING PATTERN
  found: Injects ICurrentUser, uses currentUser.UserId for cashierId (line 61) and currentUser.Email (line 62). Does NOT include user ID in command record.
  implication: Confirms pattern consistently

- timestamp: 2026-03-06T00:00:00Z
  checked: ProcessRefundHandler (line 42-49) - WORKING PATTERN
  found: Injects ICurrentUser, uses currentUser.UserId for refund.Process (line 65) and currentUser.BranchId (line 72). Does NOT include user ID in command record.
  implication: ProcessRefund follows correct pattern; RequestRefund does not

## Resolution

root_cause: ApplyDiscountHandler and RequestRefundHandler are the only billing handlers that do NOT inject ICurrentUser. Instead, they expose RequestedById as a required field on their command records, forcing the client to send it. Every other billing handler (CreateInvoice, RecordPayment, OpenShift, CloseShift, ProcessRefund) follows the pattern of injecting ICurrentUser into the handler and reading UserId/BranchId server-side.
fix: (not applied - diagnosis only)
verification: (not applied - diagnosis only)
files_changed: []
