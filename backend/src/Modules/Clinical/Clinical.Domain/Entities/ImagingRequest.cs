using Shared.Domain;

namespace Clinical.Domain.Entities;

/// <summary>
/// Imaging request created by a doctor during the exam stage.
/// Contains a list of requested imaging services (OCT, Fluorescein, etc.).
/// Triggers the DoctorExam -> Imaging -> DoctorReviewsResults loop.
/// </summary>
public class ImagingRequest : Entity
{
    public Guid VisitId { get; private set; }
    public Guid DoctorId { get; private set; }
    public string? DoctorNote { get; private set; }
    public DateTime RequestedAt { get; private set; }

    private readonly List<ImagingService> _services = [];
    public IReadOnlyCollection<ImagingService> Services => _services.AsReadOnly();

    private ImagingRequest() { }

    public static ImagingRequest Create(Guid visitId, Guid doctorId, string? doctorNote, List<string> serviceNames)
    {
        var request = new ImagingRequest
        {
            VisitId = visitId,
            DoctorId = doctorId,
            DoctorNote = doctorNote,
            RequestedAt = DateTime.UtcNow
        };

        foreach (var serviceName in serviceNames)
        {
            request._services.Add(ImagingService.Create(request.Id, serviceName, "Both"));
        }

        return request;
    }

    public bool AllServicesCompleted => _services.All(s => s.IsCompleted);
}
