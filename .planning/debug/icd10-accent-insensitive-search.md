---
status: diagnosed
trigger: "Searching for unaccented 'viem' does not return results containing accented 'viêm' in ICD-10 search."
created: 2026-03-09T08:00:00Z
updated: 2026-03-09T08:30:00Z
---

## Current Focus

hypothesis: SQL Server collation SQL_Latin1_General_CP1_CI_AS is accent-sensitive (AS), so LIKE '%viem%' never matches 'viêm' stored in the database. EF Core's Contains() translates directly to LIKE, inheriting the column collation.
test: Executed direct SQL queries against Ganka28Dev to verify
expecting: Confirmed - unaccented LIKE returns 0 rows; accented LIKE returns rows; CI_AI collation returns 1 (match)
next_action: Report root cause (diagnosis complete)

## Symptoms

expected: Typing "viem" in the ICD-10 search box returns entries whose DescriptionVi contains "viêm" (accented)
actual: Typing "viem" returns zero results for Vietnamese descriptions with diacritics
errors: No errors - the query succeeds and returns an empty set
reproduction: Call GET /api/icd10/search?term=viem (or use the ICD-10 search UI). Zero results returned for Vietnamese matches.
started: Since the seed data was updated to include proper Vietnamese diacritical marks in DescriptionVi

## Eliminated

- hypothesis: The seed data JSON still has unaccented text (old bug from previous session)
  evidence: icd10-ophthalmology.json now has proper diacritics (e.g. "Chalazion mi mắt trên phải"). Direct SQL confirmed accented rows exist in DB.
  timestamp: 2026-03-09T08:05:00Z

- hypothesis: Encoding/deserialization stripping accents during seeding
  evidence: Icd10Seeder.cs uses System.Text.Json with UTF-8 stream. SQL query with N'%viêm%' (accented) matches rows correctly, proving the data is stored with correct Unicode diacritics in the database.
  timestamp: 2026-03-09T08:10:00Z

- hypothesis: EF Core performing client-side filtering that could normalize text
  evidence: ReferenceDataRepository.SearchAsync uses .Where(...).ToListAsync() which translates to server-side SQL. No client-side normalization occurs. The problem is in SQL execution, not .NET code.
  timestamp: 2026-03-09T08:15:00Z

## Evidence

- timestamp: 2026-03-09T08:05:00Z
  checked: icd10-ophthalmology.json (first 30 lines)
  found: DescriptionVi fields now have proper Vietnamese diacritics - "Chalazion mi mắt trên phải", "Chalazion mi mắt dưới phải", etc.
  implication: The seed data problem from the previous debug session is already resolved. The bug is now different.

- timestamp: 2026-03-09T08:10:00Z
  checked: ReferenceDataRepository.cs - SearchAsync method
  found: Search uses EF Core LINQ: c.DescriptionVi.Contains(term). This translates to SQL LIKE '%{term}%' using the column's collation. No accent normalization in C# code.
  implication: The fix must happen at the database/query level, not the C# application layer.

- timestamp: 2026-03-09T08:15:00Z
  checked: ReferenceDbContext.cs - Icd10Codes entity configuration
  found: DescriptionVi is mapped as nvarchar(500) with no explicit collation override. No UseCollation() call anywhere in the EF Core model builder.
  implication: The column inherits the database's default collation.

- timestamp: 2026-03-09T08:20:00Z
  checked: SQL Server - server and database collation (via SERVERPROPERTY and DATABASEPROPERTYEX)
  found: Both server collation and database collation are SQL_Latin1_General_CP1_CI_AS
  implication: CI = Case Insensitive (good), AS = Accent Sensitive (bad for this use case). The "AS" suffix means 'e' and 'ê' are treated as DIFFERENT characters.

- timestamp: 2026-03-09T08:25:00Z
  checked: Direct SQL query - LIKE '%viem%' against DescriptionVi column
  found: SELECT TOP 3 Code, DescriptionVi FROM [reference].[Icd10Codes] WHERE DescriptionVi LIKE '%viem%' returned (0 rows affected)
  implication: Directly proves the bug. Unaccented search produces no results even though accented data exists.

- timestamp: 2026-03-09T08:26:00Z
  checked: Direct SQL query - LIKE N'%viêm%' against DescriptionVi column
  found: Returns rows: "Viêm bờ mi mắt phải...", "Viêm bờ mi mắt trái...", etc. - (3 rows affected)
  implication: The data IS there. The problem is purely collation/accent sensitivity during comparison.

- timestamp: 2026-03-09T08:28:00Z
  checked: SQL collation comparison test
  found: 'viem' COLLATE SQL_Latin1_General_CP1_CI_AS LIKE N'%viêm%' = 0 (no match). 'viêm' COLLATE Latin1_General_CI_AI LIKE '%viem%' = 1 (match).
  implication: Conclusive. The current AS (Accent Sensitive) collation blocks unaccented search. An AI (Accent Insensitive) collation resolves it. The fix does NOT require changing the database collation globally.

## Resolution

root_cause: |
  The database and all columns use the SQL_Latin1_General_CP1_CI_AS collation. The suffix "AS" means
  Accent Sensitive - SQL Server treats 'e' and 'ê' (and all Vietnamese diacritics) as distinct characters
  when comparing. EF Core's Contains(term) translates to SQL LIKE '%{term}%', which inherits this
  accent-sensitive collation. Therefore, searching for "viem" never matches stored value "viêm".

  The full chain:
  1. User types "viem" -> SearchIcd10CodesHandler receives term = "viem"
  2. ReferenceDataRepository.SearchAsync calls .Where(c => c.DescriptionVi.Contains("viem"))
  3. EF Core generates: WHERE [DescriptionVi] LIKE N'%viem%'
  4. SQL Server evaluates with column collation SQL_Latin1_General_CP1_CI_AS (accent-sensitive)
  5. 'v','i','e','m' does not equal 'v','i','ê','m' under AS rules -> 0 rows returned

  The fix options are (in increasing scope):
  A. Query-level COLLATE override (narrowest scope, recommended): Modify the EF Core query to
     apply COLLATE Latin1_General_CI_AI to the DescriptionVi comparison using EF.Functions or
     raw SQL. Only this one search query is affected. No schema changes needed.
  B. Column-level COLLATE in migration (medium scope): Add a migration that runs:
     ALTER TABLE [reference].[Icd10Codes] ALTER COLUMN [DescriptionVi] nvarchar(500)
     COLLATE Latin1_General_CI_AI NOT NULL
     This makes DescriptionVi permanently accent-insensitive.
  C. Database-level collation change (broadest scope, risky): Change the database default collation.
     Not recommended - affects all tables and may break other comparisons.

fix: (not applied - diagnosis only)
verification: (not applied - diagnosis only)
files_changed: []
