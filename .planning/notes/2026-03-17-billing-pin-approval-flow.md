---
date: "2026-03-17 15:50"
promoted: false
---

Implement full manager PIN approval flow for billing discounts and refunds. Currently bypassed — discounts auto-approve with current user. Need: (1) PIN management UI for managers to set/change PIN, (2) VerifyManagerPinHandler to verify against stored PIN hash instead of stub, (3) Re-enable ApprovalPinDialog in DiscountDialog and RefundDialog, (4) Reject discount flow with PIN.
