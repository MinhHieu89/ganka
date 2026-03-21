---
status: diagnosed
trigger: "Investigate: (1) Manager approval bypasses PIN verification, (2) Consumable selector shows both EN and VN names"
created: 2026-03-21T00:00:00Z
updated: 2026-03-21T00:00:00Z
---

## Current Focus

hypothesis: Both issues have clear root causes identified -- see Resolution
test: n/a (research only)
expecting: n/a
next_action: none -- findings documented below

## Symptoms

expected: |
  Issue 1: Cancellation approval should verify manager PIN against stored PIN in database.
  Issue 2: Consumable selector should show only the name in the currently active language.
actual: |
  Issue 1: PIN verification is a stub -- accepts ANY non-empty PIN as valid.
  Issue 2: Dropdown always shows "EnglishName (VietnameseName)" regardless of language.
errors: none (no runtime errors -- both are logic/implementation gaps)
reproduction: |
  Issue 1: Enter any 4-6 digit PIN in the approve dialog -- it will always succeed.
  Issue 2: Open consumable selector in any language -- dropdown items show both names.
started: always broken (stub was never replaced; selector was coded to show both)

## Eliminated

(none -- root causes found on first pass)

## Evidence

- timestamp: 2026-03-21
  checked: frontend/src/features/treatment/components/CancellationApprovalQueue.tsx
  found: |
    Frontend is CORRECT. Lines 52-57 define approveSchema requiring managerPin (4-6 digits).
    Lines 224-248 render a password input for PIN. Lines 129-134 send managerPin to API.
    The frontend properly collects and sends the PIN.
  implication: The issue is NOT on the frontend side.

- timestamp: 2026-03-21
  checked: backend/src/Modules/Treatment/Treatment.Application/Features/ApproveCancellation.cs
  found: |
    Lines 70-74: Handler correctly calls VerifyManagerPinQuery via messageBus and checks pinResponse.IsValid.
    The handler code is structurally correct -- it dispatches the cross-module query and rejects if invalid.
  implication: The handler wiring is correct. The problem is in the VerifyManagerPinHandler itself.

- timestamp: 2026-03-21
  checked: backend/src/Modules/Auth/Auth.Application/Features/VerifyManagerPin.cs
  found: |
    THE ROOT CAUSE for Issue 1. Lines 11-16:
    ```csharp
    public static VerifyManagerPinResponse Handle(VerifyManagerPinQuery query)
    {
        // TODO: Implement actual PIN verification against Auth.Domain.User.ManagerPin
        // For now, accept any non-empty PIN
        return new VerifyManagerPinResponse(!string.IsNullOrWhiteSpace(query.Pin));
    }
    ```
    This is a STUB. It returns IsValid=true for ANY non-empty PIN. No actual PIN lookup or hash comparison.
  implication: |
    Every module that uses VerifyManagerPinQuery (Treatment.ApproveCancellation, Billing.ApproveRefund,
    Billing.ApproveDiscount, Billing.RejectDiscount) is affected. PIN verification is theater -- any
    digits will pass.

- timestamp: 2026-03-21
  checked: backend/src/Modules/Billing/Billing.Application/Features/ApproveRefund.cs (reference pattern)
  found: |
    Lines 59-63: Uses identical pattern -- messageBus.InvokeAsync<VerifyManagerPinResponse>(new VerifyManagerPinQuery(...)).
    Same stub handler serves all callers.
  implication: Fix is centralized -- only VerifyManagerPin.cs needs real implementation.

- timestamp: 2026-03-21
  checked: frontend/src/features/treatment/components/ConsumableSelector.tsx
  found: |
    THE ROOT CAUSE for Issue 2. Two locations:

    1. DROPDOWN ITEMS (lines 151-157): Always renders both names:
       ```tsx
       <span className="text-sm">{item.name}</span>
       {item.nameVi && item.nameVi !== item.name && (
         <span className="text-xs text-muted-foreground ml-2">({item.nameVi})</span>
       )}
       ```
       This unconditionally shows EN name + VN name in parentheses, ignoring i18n language.

    2. SELECTED ITEMS LIST (lines 60-64): handleSelect stores only item.name (EN) as consumableName:
       ```tsx
       consumableName: item.name,
       ```
       This means after selection, only the English name is shown regardless of language.

    3. SEARCH FILTER (lines 49-53): Searches both name and nameVi, which is actually correct
       behavior for search (find items regardless of language typed).
  implication: |
    The component never checks i18n.language to decide which name to display.
    It has useTranslation("treatment") imported but only uses t() for labels, not for
    consumable item names.

- timestamp: 2026-03-21
  checked: frontend/src/features/consumables/api/consumables-api.ts (ConsumableItemDto)
  found: |
    Lines 5-15: API returns both `name` (EN) and `nameVi` (VN) as separate fields.
    The data is available -- the component just doesn't use language to pick which to show.
  implication: Fix requires using i18n.language to select name vs nameVi for display.

## Resolution

root_cause: |
  ISSUE 1 -- PIN BYPASS:
  The VerifyManagerPinHandler in Auth.Application is a stub that accepts any non-empty PIN.
  It never queries the database for the user's actual stored PIN hash. The TODO comment on
  line 13 confirms this was intentionally deferred. All modules using VerifyManagerPinQuery
  (Treatment, Billing) are affected.

  File: backend/src/Modules/Auth/Auth.Application/Features/VerifyManagerPin.cs

  ISSUE 2 -- DUAL LANGUAGE DISPLAY:
  ConsumableSelector.tsx hardcodes display of both item.name and item.nameVi simultaneously
  in the dropdown (lines 151-157). It also stores only item.name (English) as consumableName
  when an item is selected (line 63). The component ignores i18n.language entirely for item
  name rendering.

  File: frontend/src/features/treatment/components/ConsumableSelector.tsx

fix: |
  NOT APPLIED (research only). Suggested directions:

  Issue 1:
  - Implement real PIN verification in VerifyManagerPinHandler:
    - Inject IUserRepository (or equivalent) to look up user by ManagerId
    - Retrieve stored hashed PIN from User entity
    - Hash the provided query.Pin and compare against stored hash
    - Return IsValid based on actual comparison
  - Prerequisite: User entity must have a ManagerPin field (check if it exists)
  - Consider: PIN setup/reset flow for managers

  Issue 2:
  - Use i18n.language (from useTranslation hook already imported) to pick display name:
    - const displayName = i18n.language === 'vi' ? item.nameVi : item.name
  - Apply in dropdown CommandItem render AND in handleSelect (store localized name or both)
  - Consider storing both names in ConsumableInput so language can switch without data loss

verification: n/a (research only)
files_changed: []
