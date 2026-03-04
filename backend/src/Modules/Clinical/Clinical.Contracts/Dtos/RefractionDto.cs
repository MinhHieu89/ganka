namespace Clinical.Contracts.Dtos;

/// <summary>
/// DTO for refraction data with all per-eye measurements.
/// </summary>
public record RefractionDto(
    Guid Id,
    int Type,
    decimal? OdSph, decimal? OdCyl, decimal? OdAxis, decimal? OdAdd, decimal? OdPd,
    decimal? OsSph, decimal? OsCyl, decimal? OsAxis, decimal? OsAdd, decimal? OsPd,
    decimal? UcvaOd, decimal? UcvaOs, decimal? BcvaOd, decimal? BcvaOs,
    decimal? IopOd, decimal? IopOs, int? IopMethod,
    decimal? AxialLengthOd, decimal? AxialLengthOs);
