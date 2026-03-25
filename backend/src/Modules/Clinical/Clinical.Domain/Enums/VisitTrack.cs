namespace Clinical.Domain.Enums;

/// <summary>
/// Identifies parallel post-payment tracks for a visit.
/// After Cashier payment, the visit may have drug dispensing and/or glasses tracks running in parallel.
/// </summary>
public enum VisitTrack
{
    DrugTrack = 0,
    GlassesTrack = 1
}
