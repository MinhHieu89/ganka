using Optical.Contracts.Dtos;
using Shared.Domain;

namespace Optical.Application.Features.Orders;

/// <summary>
/// Query to retrieve a single glasses order by ID with full item details.
/// Handler implementation provided in plan 08-18.
/// </summary>
public sealed record GetGlassesOrderByIdQuery(Guid Id);
