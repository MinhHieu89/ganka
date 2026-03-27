using Patient.Domain.Enums;
using Patient.Domain.Events;
using Shared.Domain;

namespace Patient.Domain.Entities;

/// <summary>
/// Patient aggregate root. Represents a registered patient in the ophthalmology clinic.
/// Supports two registration types: Medical (full demographics) and WalkIn (name + phone only).
/// Implements IAuditable for audit log tracking.
/// Uses DDD patterns: private setters, factory method, domain events, allergy collection.
/// </summary>
public class Patient : AggregateRoot, IAuditable
{
    public string FullName { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string? PatientCode { get; private set; }
    public PatientType PatientType { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public Gender? Gender { get; private set; }
    public string? Address { get; private set; }
    public string? Cccd { get; private set; }
    public string? PhotoUrl { get; private set; }
    public int Year { get; private set; }
    public int SequenceNumber { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Intake form fields
    public string? Email { get; private set; }
    public string? Occupation { get; private set; }
    public string? OcularHistory { get; private set; }
    public string? SystemicHistory { get; private set; }
    public string? CurrentMedications { get; private set; }
    public decimal? ScreenTimeHours { get; private set; }
    public WorkEnvironment? WorkEnvironment { get; private set; }
    public ContactLensUsage? ContactLensUsage { get; private set; }
    public string? LifestyleNotes { get; private set; }

    /// <summary>
    /// Concurrency token for optimistic concurrency control.
    /// Automatically managed by SQL Server rowversion.
    /// </summary>
    public byte[] RowVersion { get; private set; } = [];

    private readonly List<Allergy> _allergies = [];
    public IReadOnlyCollection<Allergy> Allergies => _allergies.AsReadOnly();

    private Patient() { }

    /// <summary>
    /// Factory method for creating a new patient.
    /// Medical type requires DOB and gender; WalkIn type requires only name and phone.
    /// </summary>
    public static Patient Create(
        string fullName,
        string phone,
        PatientType type,
        BranchId branchId,
        DateTime? dateOfBirth = null,
        Gender? gender = null,
        string? address = null,
        string? cccd = null)
    {
        if (type == PatientType.Medical)
        {
            if (dateOfBirth is null)
                throw new ArgumentException("Date of birth is required for Medical patients.", nameof(dateOfBirth));
            if (gender is null)
                throw new ArgumentException("Gender is required for Medical patients.", nameof(gender));
        }

        var patient = new Patient
        {
            FullName = fullName,
            Phone = phone,
            PatientType = type,
            DateOfBirth = dateOfBirth,
            Gender = gender,
            Address = address,
            Cccd = cccd
        };

        patient.SetBranchId(branchId);
        patient.AddDomainEvent(new PatientRegisteredEvent
        {
            PatientId = patient.Id,
            PatientCode = string.Empty, // Will be set after code generation
            FullName = fullName
        });

        return patient;
    }

    /// <summary>
    /// Sets the patient code based on year-scoped sequence number.
    /// Called after persistence assigns the sequence number.
    /// </summary>
    public void SetPatientCode(int year)
    {
        Year = year;
        PatientCode = $"GK-{year}-{SequenceNumber:D4}";
    }

    /// <summary>
    /// Sets the year and sequence number for patient code generation.
    /// </summary>
    public void SetSequence(int year, int sequenceNumber)
    {
        Year = year;
        SequenceNumber = sequenceNumber;
        PatientCode = $"GK-{year}-{sequenceNumber:D4}";
    }

    /// <summary>
    /// Adds an allergy to this patient's allergy collection.
    /// </summary>
    public Allergy AddAllergy(string name, AllergySeverity severity)
    {
        var allergy = Allergy.Create(name, severity, Id);
        _allergies.Add(allergy);
        SetUpdatedAt();
        return allergy;
    }

    /// <summary>
    /// Removes an allergy from this patient's collection by allergy ID.
    /// </summary>
    public void RemoveAllergy(Guid allergyId)
    {
        var allergy = _allergies.FirstOrDefault(a => a.Id == allergyId);
        if (allergy is not null)
        {
            _allergies.Remove(allergy);
            SetUpdatedAt();
        }
    }

    /// <summary>
    /// Soft-delete: deactivates the patient.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    /// <summary>
    /// Reactivates a previously deactivated patient.
    /// </summary>
    public void Reactivate()
    {
        IsActive = true;
        SetUpdatedAt();
    }

    /// <summary>
    /// Updates patient demographic information.
    /// </summary>
    public void Update(
        string fullName,
        string phone,
        DateTime? dateOfBirth = null,
        Gender? gender = null,
        string? address = null,
        string? cccd = null,
        string? photoUrl = null)
    {
        FullName = fullName;
        Phone = phone;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        Address = address;
        Cccd = cccd;
        PhotoUrl = photoUrl;

        SetUpdatedAt();
        AddDomainEvent(new PatientUpdatedEvent
        {
            PatientId = Id
        });
    }

    /// <summary>
    /// Updates patient information including intake form fields.
    /// Used by receptionist during check-in intake.
    /// </summary>
    public void UpdateIntake(
        string fullName,
        string phone,
        DateTime? dateOfBirth = null,
        Gender? gender = null,
        string? address = null,
        string? cccd = null,
        string? email = null,
        string? occupation = null,
        string? ocularHistory = null,
        string? systemicHistory = null,
        string? currentMedications = null,
        decimal? screenTimeHours = null,
        WorkEnvironment? workEnvironment = null,
        ContactLensUsage? contactLensUsage = null,
        string? lifestyleNotes = null)
    {
        FullName = fullName;
        Phone = phone;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        Address = address;
        Cccd = cccd;
        Email = email;
        Occupation = occupation;
        OcularHistory = ocularHistory;
        SystemicHistory = systemicHistory;
        CurrentMedications = currentMedications;
        ScreenTimeHours = screenTimeHours;
        WorkEnvironment = workEnvironment;
        ContactLensUsage = contactLensUsage;
        LifestyleNotes = lifestyleNotes;
        SetUpdatedAt();
    }

    /// <summary>
    /// Sets the patient photo URL.
    /// </summary>
    public void SetPhotoUrl(string photoUrl)
    {
        PhotoUrl = photoUrl;
        SetUpdatedAt();
    }
}
