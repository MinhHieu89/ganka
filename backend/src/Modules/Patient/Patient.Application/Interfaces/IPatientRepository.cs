using Patient.Domain.Enums;

namespace Patient.Application.Interfaces;

/// <summary>
/// Repository interface for the Patient aggregate root.
/// </summary>
public interface IPatientRepository
{
    Task<Domain.Entities.Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Domain.Entities.Patient?> GetByIdWithTrackingAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Domain.Entities.Patient>> SearchAsync(string term, int limit = 20, CancellationToken cancellationToken = default);
    Task<(List<Domain.Entities.Patient> Patients, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        Gender? gender = null,
        bool? hasAllergies = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);
    Task<bool> PhoneExistsAsync(string phone, CancellationToken cancellationToken = default);
    Task<int> GetMaxSequenceNumberForYearAsync(int year, CancellationToken cancellationToken = default);
    Task<List<Domain.Entities.Patient>> GetRecentAsync(int count = 10, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Domain.Entities.Patient patient);
}
