---
status: resolved
trigger: "Redundant placeholders on public booking form (/book) for appointment type, preferred date, preferred time fields"
created: 2026-03-09T00:00:00Z
updated: 2026-03-09T00:00:00Z
---

## Current Focus

hypothesis: Three fields on BookingForm have placeholder text that duplicates or is redundant with existing labels
test: Read BookingForm.tsx and compare placeholder props to FieldLabel text
expecting: Placeholder values match label text exactly
next_action: Return diagnosis

## Symptoms

expected: Fields with labels should NOT have redundant placeholders (per CLAUDE.md project rule)
actual: Three fields have placeholder props set when they already have labels
errors: N/A (cosmetic/UX issue)
reproduction: Navigate to /book, observe placeholder text in appointment type select, date picker, and preferred time select
started: Since form was created

## Eliminated

(none needed - root cause found on first investigation)

## Evidence

- timestamp: 2026-03-09
  checked: BookingForm.tsx lines 170-200 (appointment type field)
  found: Line 171 has FieldLabel with t("appointmentType") = "Loai lich hen"; Line 178 has SelectValue placeholder={t("appointmentType")} = same text
  implication: Placeholder duplicates label exactly

- timestamp: 2026-03-09
  checked: BookingForm.tsx lines 221-246 (preferred time field)
  found: Line 223 has FieldLabel with t("selfBooking.preferredTime") = "Gio mong muon"; Line 230 has SelectValue placeholder={t("selfBooking.preferredTime")} = same text
  implication: Placeholder duplicates label exactly

- timestamp: 2026-03-09
  checked: BookingForm.tsx lines 202-219 (preferred date field) + DatePicker.tsx
  found: Line 204 has FieldLabel with t("selfBooking.preferredDate") = "Ngay mong muon"; DatePicker has no explicit placeholder passed so defaults to t("buttons.search") = "Tim kiem"
  implication: DatePicker default placeholder is "Tim kiem" (Search) not matching label, but still a meaningless placeholder on a date field with a label

## Resolution

root_cause: |
  In BookingForm.tsx, two Select fields pass placeholder props to SelectValue that are identical
  to their FieldLabel text, violating the project rule "Only add placeholder where it makes sense,
  don't add it to all inputs by default":

  1. Line 178: placeholder={t("appointmentType")} duplicates label on line 171
  2. Line 230: placeholder={t("selfBooking.preferredTime")} duplicates label on line 223

  For the DatePicker (line 209-213), no explicit placeholder is passed, so it defaults to
  t("buttons.search") = "Tim kiem" inside DatePicker.tsx line 40. This is not a label duplicate
  but is still a meaningless placeholder for a date picker that already has a label.

fix: Remove placeholder props from the two SelectValue components; optionally remove/fix DatePicker default
verification: (pending)
files_changed: []
