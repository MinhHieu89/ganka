using Optical.Contracts.Dtos;
using Shared.Domain;

namespace Optical.Application.Features.Lenses;

/// <summary>
/// Query to retrieve the full lens catalog with stock entries.
/// Handler implementation provided in plan 08-17.
/// </summary>
public sealed record GetLensCatalogQuery(bool IncludeInactive = false);
