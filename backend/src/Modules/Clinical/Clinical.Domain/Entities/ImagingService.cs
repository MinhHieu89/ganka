using Shared.Domain;

namespace Clinical.Domain.Entities;

/// <summary>
/// Individual imaging service within an ImagingRequest (e.g., OCT, Fluorescein).
/// Tracks completion status per service per eye scope.
/// </summary>
public class ImagingService : Entity
{
    public Guid ImagingRequestId { get; private set; }
    public string ServiceName { get; private set; } = string.Empty;
    public string EyeScope { get; private set; } = string.Empty; // OD, OS, Both
    public bool IsCompleted { get; private set; }
    public string? TechnicianNote { get; private set; }

    private ImagingService() { }

    public static ImagingService Create(Guid imagingRequestId, string serviceName, string eyeScope)
    {
        return new ImagingService
        {
            ImagingRequestId = imagingRequestId,
            ServiceName = serviceName,
            EyeScope = eyeScope,
            IsCompleted = false
        };
    }

    public void MarkCompleted(string? technicianNote = null)
    {
        IsCompleted = true;
        TechnicianNote = technicianNote;
        SetUpdatedAt();
    }
}
