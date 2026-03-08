using Optical.Contracts.Dtos;
using Shared.Domain;

namespace Optical.Application.Features.Frames;

/// <summary>
/// Query to retrieve a single frame by ID.
/// Handler implementation provided in plan 08-16.
/// </summary>
public sealed record GetFrameByIdQuery(Guid Id);
