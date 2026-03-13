---
phase: quick
plan: 1
type: execute
wave: 1
depends_on: []
files_modified:
  - backend/src/Shared/Shared.Presentation/ResultExtensions.cs
  - backend/tests/Shared.Unit.Tests/ResultExtensionsTests.cs
autonomous: true
requirements: [FIX-CREATED-RESULT]
must_haves:
  truths:
    - "ToCreatedHttpResult<Guid> returns 201 with { id: <guid> } and location header (unchanged)"
    - "ToCreatedHttpResult<T> where T is a DTO returns 201 with the DTO directly as the body"
    - "ToCreatedHttpResult<T> where T is a DTO does NOT include a location header"
    - "All 30+ existing callers continue working without changes"
  artifacts:
    - path: "backend/src/Shared/Shared.Presentation/ResultExtensions.cs"
      provides: "Fixed ToCreatedHttpResult<T> with type branching"
      contains: "typeof(T) == typeof(Guid)"
    - path: "backend/tests/Shared.Unit.Tests/ResultExtensionsTests.cs"
      provides: "Tests for both Guid and DTO code paths"
  key_links:
    - from: "ResultExtensions.cs"
      to: "All *ApiEndpoints.cs callers"
      via: "extension method ToCreatedHttpResult<T>"
      pattern: "ToCreatedHttpResult"
---

<objective>
Fix `ToCreatedHttpResult<T>` in `ResultExtensions.cs` so that when T is a DTO (not Guid), the 201 response body contains the DTO directly instead of wrapping it in `new { Id = result.Value }`.

Purpose: Currently all create endpoints that return DTOs (invoices, payments, treatment protocols, etc.) wrap the DTO in an unnecessary `{ id: {...dto...} }` object, forcing frontend to unwrap or making the response unusable.

Output: Fixed method with type branching, updated tests covering both Guid and DTO paths.
</objective>

<execution_context>
@C:/Users/minhh/.claude/get-shit-done/workflows/execute-plan.md
@C:/Users/minhh/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@backend/src/Shared/Shared.Presentation/ResultExtensions.cs
@backend/tests/Shared.Unit.Tests/ResultExtensionsTests.cs

<interfaces>
From backend/src/Shared/Shared.Presentation/ResultExtensions.cs (line 41-47):
```csharp
public static IResult ToCreatedHttpResult<T>(this Result<T> result, string routePrefix)
{
    if (result.IsSuccess)
        return Results.Created($"{routePrefix}/{result.Value}", new { Id = result.Value });

    return MapError(result.Error);
}
```
</interfaces>
</context>

<tasks>

<task type="auto" tdd="true">
  <name>Task 1: Add failing tests for DTO path in ToCreatedHttpResult</name>
  <files>backend/tests/Shared.Unit.Tests/ResultExtensionsTests.cs</files>
  <behavior>
    - Test: ToCreatedHttpResult with Guid returns Created with `{ Id = guid }` body and location header `routePrefix/guid` (existing behavior, ensure covered)
    - Test: ToCreatedHttpResult with a DTO record type returns Created with the DTO directly as the body (no wrapping)
    - Test: ToCreatedHttpResult with a DTO record type returns 201 status with Results.Ok-style body (no location header)
    - Test: ToCreatedHttpResult with Guid failure still maps errors correctly (existing test should pass)
  </behavior>
  <action>
Add a simple test record at the top of the test class: `private record TestDto(Guid Id, string Name);`

Add these test methods:
1. `ToCreatedHttpResult_Guid_Success_ReturnsCreatedWithIdAndLocation` — create `Result<Guid>.Success(someGuid)`, call `.ToCreatedHttpResult("/api/test")`, assert result is `Created<object>` with body containing Id property equal to the guid and location containing the guid.
2. `ToCreatedHttpResult_Dto_Success_ReturnsDtoDirectly` — create `Result<TestDto>.Success(new TestDto(someGuid, "Test"))`, call `.ToCreatedHttpResult("/api/test")`, assert the response body IS the TestDto (not wrapped), status is 201.
3. `ToCreatedHttpResult_Dto_Success_DoesNotIncludeLocationWithGuid` — verify location header does NOT contain the guid string interpolation pattern (since DTO.ToString() would be meaningless in a URL).

Run tests — the DTO tests MUST FAIL (red phase) because current implementation wraps everything in `new { Id = ... }`.

Note: In .NET 10 minimal APIs, `Results.Created(uri, value)` returns `Created<T>`. `Results.Json(value, statusCode: 201)` or `Results.Ok(value)` with status override may be used. Check the actual return types when asserting. Use `TypedResults` for predictable return types in assertions if needed.
  </action>
  <verify>
    <automated>cd D:/projects/ganka/backend && dotnet test tests/Shared.Unit.Tests/Shared.Unit.Tests.csproj --filter "ToCreatedHttpResult_Dto" --no-restore 2>&1 | tail -5</automated>
  </verify>
  <done>New DTO-path tests exist and FAIL (red), confirming the bug is captured by tests</done>
</task>

<task type="auto">
  <name>Task 2: Fix ToCreatedHttpResult to branch on type and make tests green</name>
  <files>backend/src/Shared/Shared.Presentation/ResultExtensions.cs</files>
  <action>
Modify `ToCreatedHttpResult<T>` method (line 41-47) to branch based on `typeof(T)`:

```csharp
public static IResult ToCreatedHttpResult<T>(this Result<T> result, string routePrefix)
{
    if (result.IsSuccess)
    {
        if (typeof(T) == typeof(Guid))
        {
            // Guid path: return location header + wrapped Id object
            return Results.Created($"{routePrefix}/{result.Value}", new { Id = result.Value });
        }

        // DTO path: return the DTO directly as 201 body, no location header
        return TypedResults.Created((string?)null, result.Value);
    }

    return MapError(result.Error);
}
```

Key decisions per user context:
- Guid gets location header + `{ Id = guid }` body (unchanged behavior)
- DTO gets no location header, body is the DTO directly (flat, no wrapping)
- Using `typeof(T) == typeof(Guid)` compile-time type check (no reflection overhead)
- `TypedResults.Created((string?)null, result.Value)` gives 201 status with DTO body and no location

After implementation, run ALL ResultExtensions tests to ensure both old and new tests pass (green phase).
  </action>
  <verify>
    <automated>cd D:/projects/ganka/backend && dotnet test tests/Shared.Unit.Tests/Shared.Unit.Tests.csproj --no-restore 2>&1 | tail -10</automated>
  </verify>
  <done>All tests pass: Guid path returns 201 with wrapped Id + location, DTO path returns 201 with DTO directly, error paths unchanged</done>
</task>

</tasks>

<verification>
- All existing ResultExtensionsTests pass (no regression)
- New DTO-path tests pass (bug is fixed)
- `dotnet build` succeeds for the entire backend solution (no compilation errors from callers)
</verification>

<success_criteria>
- `ToCreatedHttpResult<Guid>` returns `{ id: "<guid>" }` with location header (unchanged)
- `ToCreatedHttpResult<SomeDto>` returns the DTO flat as 201 body (no wrapping)
- All 30+ callers across modules compile and work without modification
- All unit tests pass with >= 80% coverage on ResultExtensions
</success_criteria>

<output>
After completion, create `.planning/quick/1-fix-tocreatedhttpresult-wrapping-issue-r/1-SUMMARY.md`
</output>
