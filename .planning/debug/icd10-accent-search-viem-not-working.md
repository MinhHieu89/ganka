# ICD-10 Accent-Insensitive Search: "viem" Not Working

**Found during:** 03-16 Task 2 human verification
**Date:** 2026-03-09
**Severity:** HIGH
**Phase:** 03-clinical-workflow-examination

## Problem

Searching "viem" (no accents) in the ICD-10 diagnosis search does not return results with accented Vietnamese descriptions (e.g., entries containing "Viem" with diacritics).

## Context

Plan 03-15 added `EF.Functions.Collate(c.DescriptionVi, "Latin1_General_CI_AI")` in `ReferenceDataRepository.cs` line 26. This was verified at the code level to compile correctly and unit tests pass (SQLite). However, human verification shows the accent-insensitive search is not working at runtime with SQL Server.

## Possible Causes

1. The collation `Latin1_General_CI_AI` may not handle Vietnamese diacritics correctly (it is designed for Latin1 characters, Vietnamese uses extended Unicode)
2. The backend may need a different collation such as `Vietnamese_CI_AI` or `SQL_Latin1_General_CP1_CI_AI`
3. The search term or database column encoding may have issues

## Files

- `backend/src/Shared/Shared.Infrastructure/Repositories/ReferenceDataRepository.cs` (line 26)
- `backend/src/Modules/Audit/Audit.Infrastructure/Seeding/icd10-ophthalmology.json`

## Needs

Investigation of proper SQL Server collation for Vietnamese diacritics, then fix and runtime verification.
