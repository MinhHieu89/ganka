---
status: resolved
trigger: "drug search dropdown on stock import page doesn't filter results by search term"
created: 2026-03-22T00:00:00Z
updated: 2026-03-22T00:02:00Z
---

## Current Focus

hypothesis: CONFIRMED AND VERIFIED - Three compounding issues: missing CommandList, reliance on cmdk built-in filtering, and server-side pagination limiting results to 20
test: Playwright verification on both /pharmacy/stock-import and /pharmacy/otc-sales
expecting: Search "para" shows Paracetamol and Proparacaine
next_action: Archive session

## Symptoms

expected: When typing "para" in the drug search field, only drugs whose name contains "para" should appear
actual: The dropdown shows all drugs regardless of search term on both stock-import and otc-sales pages
errors: No error messages - just wrong search results
reproduction: Go to /pharmacy/stock-import or /pharmacy/otc-sales -> add a line -> type in drug search -> results don't filter
started: Unknown

## Eliminated

- hypothesis: Missing CommandList wrapper is the sole cause
  evidence: Added CommandList in first fix attempt but filtering still didn't work. cmdk built-in filtering also unreliable.
  timestamp: 2026-03-22T00:00:30Z

- hypothesis: Client-side filtering with shouldFilter={false} is sufficient
  evidence: useDrugCatalogList() calls API with empty term which only returns first 20 drugs (server-side limit). Drugs beyond the first 20 like Paracetamol could never be found with client-side filtering alone.
  timestamp: 2026-03-22T00:01:00Z

## Evidence

- timestamp: 2026-03-22T00:00:10Z
  checked: StockImportForm.tsx inline DrugCombobox
  found: Used cmdk built-in filtering (no shouldFilter={false}), missing CommandList
  implication: Need to disable cmdk filtering and add CommandList

- timestamp: 2026-03-22T00:00:20Z
  checked: OtcSaleForm.tsx inline DrugCombobox
  found: Had shouldFilter={false} and manual filtering BUT was missing CommandList wrapper
  implication: Both issues needed fixing

- timestamp: 2026-03-22T00:00:25Z
  checked: Working comboboxes (clinical/DrugCombobox, AllergyForm, ConsumableSelector)
  found: ALL use shouldFilter={false} + CommandList + server-side or manual filtering
  implication: Established pattern in the codebase

- timestamp: 2026-03-22T00:01:00Z
  checked: useDrugCatalogList() API call
  found: Calls /api/pharmacy/drugs/search with term="" which returns only first 20 drugs due to server-side pagination
  implication: Client-side filtering alone can never find drugs beyond the first page

- timestamp: 2026-03-22T00:01:30Z
  checked: Playwright tests on both pages
  found: Search "para" correctly shows Paracetamol and Proparacaine, selection persists in combobox display
  implication: Fix verified end-to-end

## Resolution

root_cause: Three compounding issues: (1) Missing CommandList wrapper prevented cmdk from managing item visibility. (2) StockImportForm relied on cmdk built-in filtering instead of shouldFilter={false}. (3) useDrugCatalogList() only returned first 20 drugs from server, so drugs like Paracetamol beyond that limit could never appear regardless of client-side filtering. Both forms also had duplicated inline DrugCombobox implementations.
fix: Created shared DrugCombobox component using server-side search via useDrugCatalogSearch(search) when 2+ chars typed, falling back to useDrugCatalogList() for initial display. Added pickedDrug local state to remember selected drug after search clears. Uses shouldFilter={false} + CommandList. Updated both StockImportForm and OtcSaleForm to use shared component.
verification: Playwright tests confirm search "para" shows Paracetamol and Proparacaine on both stock-import and otc-sales pages. Drug selection persists correctly in combobox display.
files_changed: [frontend/src/features/pharmacy/components/DrugCombobox.tsx, frontend/src/features/pharmacy/components/StockImportForm.tsx, frontend/src/features/pharmacy/components/OtcSaleForm.tsx]
