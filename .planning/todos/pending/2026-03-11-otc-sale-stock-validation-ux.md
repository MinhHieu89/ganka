---
created: 2026-03-11
phase: 06-pharmacy-consumables
source: UAT session
priority: normal
---

In /pharmacy/otc-sales, when submitting a drug that is not in stock, it shows a generic toast "Failed to create OTC sale". It should show a "no stock" message under the row whenever a drug is selected or quantity changed (inline validation, not just on submit).
