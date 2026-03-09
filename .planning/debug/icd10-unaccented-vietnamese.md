---
status: diagnosed
trigger: "ICD-10 search shows unaccented Vietnamese"
created: 2026-03-09T00:00:00Z
updated: 2026-03-09T00:00:00Z
---

## Current Focus

hypothesis: Vietnamese descriptions in the seed data JSON file were written without diacritical marks
test: Read the icd10-ophthalmology.json file and inspect descriptionVi values
expecting: All Vietnamese text lacks diacritics (e.g., "Viem" instead of "Viem" with proper marks)
next_action: Report root cause

## Symptoms

expected: ICD-10 search results show Vietnamese descriptions with proper diacritical marks (e.g., "Viêm kết mạc dị ứng cấp tính")
actual: Vietnamese descriptions appear unaccented (e.g., "Viem ket mac di ung cap tinh")
errors: None - data displays, just without accents
reproduction: Search any ICD-10 code and observe the Vietnamese description
started: Since initial seeding

## Eliminated

(none needed - root cause identified on first inspection)

## Evidence

- timestamp: 2026-03-09T00:00:00Z
  checked: icd10-ophthalmology.json (all 130 entries)
  found: Every single descriptionVi value uses unaccented ASCII Vietnamese. Examples:
    - "Chalazion mi mat tren phai" instead of "Chalazion mi mắt trên phải"
    - "Viem ket mac di ung cap tinh" instead of "Viêm kết mạc dị ứng cấp tính"
    - "Duc thuy tinh the vo do tuoi" instead of "Đục thủy tinh thể vỏ do tuổi"
    - "Glaucoma goc mo nguyen phat" instead of "Glaucoma góc mở nguyên phát"
    - "Can thi" instead of "Cận thị"
    - "Loan thi" instead of "Loạn thị"
    - "Lao thi" instead of "Lão thị"
  implication: The source data itself is the problem - no transformation or encoding issue in the pipeline

- timestamp: 2026-03-09T00:00:00Z
  checked: Icd10Seeder.cs - seeding pipeline
  found: Seeder reads JSON, deserializes, and inserts directly. No text transformation that could strip accents. Uses System.Text.Json with camelCase naming policy.
  implication: The seeder faithfully preserves whatever text is in the JSON - it does not strip or transform diacritics

- timestamp: 2026-03-09T00:00:00Z
  checked: Icd10Seeder.cs - idempotency logic (lines 38-51)
  found: Seeder skips codes that already exist in DB (checks by Code primary key). It does NOT update existing records.
  implication: Simply fixing the JSON file will NOT fix existing database records. A migration or manual DB update is also needed.

- timestamp: 2026-03-09T00:00:00Z
  checked: ReferenceDbContext.cs - column configuration
  found: DescriptionVi is nvarchar(500). No encoding issues at the DB schema level - nvarchar fully supports Vietnamese Unicode.
  implication: Database schema is fine; the data content is the problem

- timestamp: 2026-03-09T00:00:00Z
  checked: SearchIcd10Codes.cs and ReferenceDataRepository.cs - query pipeline
  found: Search queries DescriptionVi with Contains(). Results are mapped directly to DTOs with no transformation.
  implication: No processing step strips accents - what's in the DB is what the user sees

## Resolution

root_cause: The seed data file `icd10-ophthalmology.json` contains Vietnamese descriptions written entirely in unaccented ASCII (e.g., "Viem ket mac" instead of "Viêm kết mạc"). All 130 ICD-10 entries are affected. The seeder faithfully inserts this unaccented text into the database. Additionally, the seeder is idempotent (skip-if-exists), so fixing the JSON alone won't update already-seeded records.
fix: (not applied - diagnosis only)
verification: (not applied - diagnosis only)
files_changed: []
