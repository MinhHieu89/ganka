namespace Shared.Domain;

/// <summary>
/// Represents a typed error with a code and description.
/// Used with the Result pattern for expected failure handling without exceptions.
/// </summary>
public sealed record Error
{
    public string Code { get; }
    public string Description { get; }

    private Error(string code, string description)
    {
        Code = code;
        Description = description;
    }

    /// <summary>Represents no error (success state).</summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>Represents a null value error.</summary>
    public static readonly Error NullValue = new("Error.NullValue", "A required value was null.");

    /// <summary>Creates a validation error.</summary>
    public static Error Validation(string description) =>
        new("Error.Validation", description);

    /// <summary>Creates a not-found error for a specific entity.</summary>
    public static Error NotFound(string entityName, object id) =>
        new("Error.NotFound", $"{entityName} with id '{id}' was not found.");

    /// <summary>Creates an unauthorized access error.</summary>
    public static Error Unauthorized() =>
        new("Error.Unauthorized", "Unauthorized access.");

    /// <summary>Creates a conflict error (e.g., duplicate resource).</summary>
    public static Error Conflict(string description) =>
        new("Error.Conflict", description);

    /// <summary>Creates a custom error with the specified code and description.</summary>
    public static Error Custom(string code, string description) =>
        new(code, description);

    public bool Equals(Error? other) => other is not null && Code == other.Code;

    public override int GetHashCode() => Code.GetHashCode();
}
