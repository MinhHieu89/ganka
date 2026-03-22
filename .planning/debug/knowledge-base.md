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

