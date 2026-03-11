---
created: 2026-03-11T11:27:00.000Z
title: Stock import template should be csv or xlsx not tsv
area: ui
files:
  - frontend/src/features/pharmacy/components/StockImportPage.tsx
---

## Problem

The downloadable template file for bulk stock import is in .tsv (tab-separated values) format. Users expect .csv or .xlsx formats which are more universally supported by spreadsheet applications like Excel and Google Sheets. TSV is uncommon and may confuse users.

## Solution

Change the template download to generate either .csv or .xlsx format. Prefer .xlsx if an Excel library is already in use (e.g., exceljs, xlsx), otherwise .csv is sufficient. Update the file upload parser accordingly if needed.
