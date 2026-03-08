using Shared.Domain;

namespace Optical.Application.Features.Stocktaking;

/// <summary>
/// Command to start a new barcode-based stocktaking session.
/// Fails if another session is already in progress.
/// Handler implementation provided in plan 08-20.
/// </summary>
public sealed record StartStocktakingSessionCommand(string Name, string? Notes);
