using Shared.Domain;

namespace Patient.Domain.ValueObjects;

/// <summary>
/// Value object representing a patient code in GK-YYYY-NNNN format.
/// Year-scoped sequential numbering for ophthalmology clinic patient identification.
/// </summary>
public sealed class PatientCode : ValueObject
{
    public string Value { get; private set; } = string.Empty;

    private PatientCode() { }

    /// <summary>
    /// Creates a formatted patient code: GK-{year}-{sequenceNumber:D4}
    /// </summary>
    public static PatientCode Create(int year, int sequenceNumber)
    {
        return new PatientCode
        {
            Value = $"GK-{year}-{sequenceNumber:D4}"
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
