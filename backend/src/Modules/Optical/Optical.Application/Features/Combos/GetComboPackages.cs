using Optical.Contracts.Dtos;
using Shared.Domain;

namespace Optical.Application.Features.Combos;

/// <summary>
/// Query to retrieve all combo packages with optional inactive filter.
/// Handler implementation provided in plan 08-19.
/// </summary>
public sealed record GetComboPackagesQuery(bool IncludeInactive = false);
