using Optical.Contracts.Dtos;
using Shared.Domain;

namespace Optical.Application.Features.Stocktaking;

/// <summary>
/// Query to generate a discrepancy report for a completed stocktaking session.
/// Handler implementation provided in plan 08-20.
/// </summary>
public sealed record GetDiscrepancyReportQuery(Guid SessionId);
