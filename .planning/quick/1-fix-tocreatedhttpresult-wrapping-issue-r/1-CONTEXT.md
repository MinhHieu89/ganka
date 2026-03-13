# Quick Task 1: Fix ToCreatedHttpResult wrapping issue — Context

**Gathered:** 2026-03-13
**Status:** Ready for planning

<domain>
## Task Boundary

Fix ToCreatedHttpResult wrapping issue — response returns `{"id": {...dto...}}` instead of returning the DTO directly when T is a DTO type. Currently line 44 of ResultExtensions.cs always wraps `result.Value` in `new { Id = result.Value }`, which breaks when T is not a Guid.

</domain>

<decisions>
## Implementation Decisions

### Response body format
- When T is a DTO, return the DTO directly as the 201 response body (flat, no wrapping)
- Frontend gets the created resource immediately without a follow-up GET

### Location header behavior
- Claude's Discretion: Recommended approach is Guid gets location header, DTO gets no location header (avoids reflection complexity)

### Scope of fix
- Fix only the shared `ToCreatedHttpResult` method in `ResultExtensions.cs`
- All 30+ callers across modules automatically get the fix — no caller changes needed

### Claude's Discretion
- Location header strategy (recommended: Guid=location, DTO=no location)

</decisions>

<specifics>
## Specific Ideas

- The method signature is `ToCreatedHttpResult<T>(this Result<T> result, string routePrefix)`
- When `T` is `Guid`, current behavior (`new { Id = result.Value }` + location header) is correct
- When `T` is a DTO (InvoiceDto, PaymentDto, TreatmentProtocolDto, etc.), body should be `result.Value` directly
- Type check: `typeof(T) == typeof(Guid)` to branch behavior
- Existing unit tests in `ResultExtensionsTests.cs` need updating

</specifics>
