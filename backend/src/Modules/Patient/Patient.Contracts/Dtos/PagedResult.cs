namespace Patient.Contracts.Dtos;

/// <summary>
/// Generic paginated result container.
/// </summary>
public sealed record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize);
