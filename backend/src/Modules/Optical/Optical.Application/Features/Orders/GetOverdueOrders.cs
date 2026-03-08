using Optical.Contracts.Dtos;
using Shared.Domain;

namespace Optical.Application.Features.Orders;

/// <summary>
/// Query to retrieve glasses orders that are past their estimated delivery date and not yet delivered.
/// Handler implementation provided in plan 08-18.
/// </summary>
public sealed record GetOverdueOrdersQuery();
