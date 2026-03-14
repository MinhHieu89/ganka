# Deferred Items - Phase 09.1

## Pre-existing Test Failures

### TreatmentPackageDomainTests.RecordSession_IntervalSatisfied_Succeeds
- **File:** `backend/tests/Treatment.Unit.Tests/Domain/TreatmentPackageDomainTests.cs:109`
- **Issue:** Test uses a hard-coded `scheduledAt` date that is now in the past relative to the first session's date, causing the interval check to fail with negative days (-15). The test was written with dates that worked at the time but are now stale.
- **Fix needed:** Update the test to use relative dates (e.g., `DateTime.UtcNow.AddDays(...)`) instead of hard-coded dates.
- **Not caused by:** Plan 09.1-02 changes.
