# GSD Debug Knowledge Base

Resolved debug sessions. Used by `gsd-debugger` to surface known-pattern hypotheses at the start of new investigations.

---

## allergy-dropdown-scroll — Mouse wheel scroll not working in allergy name dropdown inside Dialog
- **Date:** 2026-03-22
- **Error patterns:** mouse wheel, scroll, dropdown, Popover, CommandList, Dialog, overflow
- **Root cause:** Radix Dialog's react-remove-scroll intercepts wheel events globally. Popover portals nested inside Dialog render outside the Dialog DOM subtree, so wheel events on CommandList get blocked by the scroll lock.
- **Fix:** Add onWheel={(e) => e.stopPropagation()} to PopoverContent to prevent Dialog's scroll lock from intercepting wheel events.
- **Files changed:** frontend/src/features/patient/components/PatientRegistrationForm.tsx, frontend/src/features/patient/components/AllergyForm.tsx
---

## patient-registration-duplicate-phone-error — Duplicate phone shows generic error banner instead of field-level error
- **Date:** 2026-03-22
- **Error patterns:** Failed to register patient, duplicate phone, generic error banner, field-level validation, Error.Conflict, RFC 7807
- **Root cause:** Backend Error.Conflict mapped to non-standard { error: "..." } response shape via Results.Conflict(). Frontend expected RFC 7807 { detail, title, errors } and fell back to generic "Failed to register patient" message, losing the descriptive phone duplicate message entirely.
- **Fix:** Changed backend to return duplicate phone as Error.ValidationWithDetails with Phone field key (RFC 7807 structured validation). Fixed ResultExtensions Conflict mapping to use standard Problem format. Frontend reordered to let handleServerValidationError set field error, then override with localized message.
- **Files changed:** backend/src/Modules/Patient/Patient.Application/Features/RegisterPatient.cs, backend/src/Shared/Shared.Presentation/ResultExtensions.cs, frontend/src/features/patient/components/PatientRegistrationForm.tsx
---

## drug-search-not-filtering — Drug search dropdown doesn't filter results on stock-import and otc-sales pages
- **Date:** 2026-03-22
- **Error patterns:** drug search, dropdown, filter not working, all drugs shown, CommandList missing, shouldFilter, server-side pagination, useDrugCatalogList, cmdk
- **Root cause:** Three compounding issues: (1) Missing CommandList wrapper prevented cmdk from managing item visibility. (2) StockImportForm relied on cmdk built-in filtering instead of shouldFilter={false}. (3) useDrugCatalogList() only returned first 20 drugs from server due to pagination, so drugs beyond that limit could never appear regardless of client-side filtering.
- **Fix:** Created shared DrugCombobox component using server-side search via useDrugCatalogSearch() when 2+ chars typed, shouldFilter={false}, CommandList, and pickedDrug local state to persist selection display after search clears. Removed duplicated inline implementations from both forms.
- **Files changed:** frontend/src/features/pharmacy/components/DrugCombobox.tsx, frontend/src/features/pharmacy/components/StockImportForm.tsx, frontend/src/features/pharmacy/components/OtcSaleForm.tsx
---

