using Shared.Domain;

namespace Optical.Application.Features.Frames;

/// <summary>
/// Command to generate and assign an EAN-13 barcode to a frame.
/// Handler implementation provided in plan 08-16.
/// </summary>
public sealed record GenerateBarcodeCommand(Guid FrameId);
