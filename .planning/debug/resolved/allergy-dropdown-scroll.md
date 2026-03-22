---
status: resolved
trigger: "allergy-dropdown-scroll - Mouse scroll wheel does not work in allergy name dropdown"
created: 2026-03-22T00:00:00Z
updated: 2026-03-22T00:00:00Z
---

## Current Focus

hypothesis: Radix Dialog's scroll lock (react-remove-scroll) intercepts wheel events on Popover portals nested inside the Dialog, preventing CommandList from scrolling
test: Add onWheel stopPropagation to PopoverContent in allergy comboboxes
expecting: Mouse wheel scrolls through allergy options normally
next_action: Apply fix to PopoverContent in both AllergyForm.tsx and PatientRegistrationForm.tsx

## Symptoms

expected: Mouse scroll wheel should scroll through the dropdown options list
actual: Mouse scroll wheel does nothing when hovering over the dropdown options list
errors: No error messages - UI interaction issue
reproduction: Open patient registration form → click "Thêm dị ứng" → open allergy name dropdown → try scrolling with mouse wheel
started: Unknown, likely since implementation

## Eliminated

## Evidence

- timestamp: 2026-03-22T00:01:00Z
  checked: AllergyForm.tsx and PatientRegistrationForm.tsx AllergyRow component
  found: Both use Popover > Command > CommandList pattern inside a Dialog
  implication: The allergy combobox is a Popover portal nested inside a Dialog portal

- timestamp: 2026-03-22T00:02:00Z
  checked: CommandList component in command.tsx
  found: Has max-h-[300px] overflow-y-auto overflow-x-hidden - CSS is correct for scrolling
  implication: The issue is not CSS-related, it's event-related

- timestamp: 2026-03-22T00:03:00Z
  checked: Popover component uses PopoverPrimitive.Portal
  found: Renders content in a separate portal outside the Dialog DOM tree
  implication: Radix Dialog's scroll lock (react-remove-scroll) blocks wheel events on elements in other portals

## Resolution

root_cause: Radix Dialog uses react-remove-scroll which intercepts wheel events globally. When a Popover renders its content via a Portal (outside the Dialog's DOM subtree), the Dialog's scroll lock blocks wheel events from reaching the CommandList's scrollable container. The CommandList has correct CSS (overflow-y-auto, max-height) but never receives the wheel events.
fix: Add onWheel={e => e.stopPropagation()} to PopoverContent in both AllergyForm.tsx and PatientRegistrationForm.tsx to prevent the Dialog's scroll lock from intercepting wheel events
verification: User confirmed mouse wheel scrolling works in allergy dropdown after fix applied
files_changed: [frontend/src/features/patient/components/PatientRegistrationForm.tsx, frontend/src/features/patient/components/AllergyForm.tsx]
