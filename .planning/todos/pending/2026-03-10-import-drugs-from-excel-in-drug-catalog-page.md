---
created: 2026-03-10T09:10:00.000Z
title: Import drugs from Excel in drug catalog page
area: ui
files:
  - frontend/src/features/pharmacy/components/DrugCatalogPage.tsx
---

## Problem

Currently drugs can only be added one at a time via the Add Drug form on /pharmacy/drug-catalog. There's no bulk import option, which makes initial catalog setup or large updates tedious.

## Solution

Add an "Import" button to the DrugCatalogPage header. When clicked, show a dialog with:
1. File upload area for Excel (.xlsx) files
2. A link to download an Excel template with the expected columns (name, generic name, form, route, unit, dosage template, etc.)
3. Validation feedback showing which rows succeeded/failed after upload
