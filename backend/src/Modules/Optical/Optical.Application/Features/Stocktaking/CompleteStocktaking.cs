using Shared.Domain;

namespace Optical.Application.Features.Stocktaking;

/// <summary>
/// Command to mark a stocktaking session as complete.
/// Handler implementation provided in plan 08-20.
/// </summary>
public sealed record CompleteStocktakingCommand(Guid SessionId, string? Notes);
