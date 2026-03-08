using Shared.Domain;

namespace Optical.Application.Features.Warranty;

/// <summary>
/// Command to file a new warranty claim for a glasses order.
/// Handler validates order is within 12-month warranty period.
/// Handler implementation provided in plan 08-19.
/// </summary>
public sealed record CreateWarrantyClaimCommand(
    Guid GlassesOrderId,
    int Resolution,
    string AssessmentNotes,
    decimal? DiscountAmount);
