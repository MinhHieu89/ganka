using Shared.Domain;

namespace Clinical.Domain.Entities;

/// <summary>
/// Optical order for frame/lens selection at OpticalCenter stage.
/// Created after glasses prescription, before Cashier payment.
/// </summary>
public class OpticalOrder : Entity
{
    public Guid VisitId { get; private set; }
    public string LensType { get; private set; } = string.Empty;
    public string FrameCode { get; private set; } = string.Empty;
    public decimal LensCostPerUnit { get; private set; }
    public decimal FrameCost { get; private set; }
    public decimal TotalPrice { get; private set; }
    public Guid ConsultantId { get; private set; }
    public string ConsultantName { get; private set; } = string.Empty;
    public DateTime ConfirmedAt { get; private set; }

    private OpticalOrder() { }

    public static OpticalOrder Create(Guid visitId, string lensType, string frameCode,
        decimal lensCostPerUnit, decimal frameCost, decimal totalPrice,
        Guid consultantId, string consultantName)
    {
        return new OpticalOrder
        {
            VisitId = visitId,
            LensType = lensType,
            FrameCode = frameCode,
            LensCostPerUnit = lensCostPerUnit,
            FrameCost = frameCost,
            TotalPrice = totalPrice,
            ConsultantId = consultantId,
            ConsultantName = consultantName,
            ConfirmedAt = DateTime.UtcNow
        };
    }
}
