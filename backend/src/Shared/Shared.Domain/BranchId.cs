namespace Shared.Domain;

/// <summary>
/// Strongly-typed identifier for multi-tenant branch isolation.
/// All aggregate roots carry a BranchId for tenant-aware query filtering.
/// </summary>
public readonly record struct BranchId
{
    public Guid Value { get; }

    public BranchId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("BranchId cannot be empty.", nameof(value));

        Value = value;
    }

    public static BranchId New() => new(Guid.NewGuid());

    public static implicit operator Guid(BranchId branchId) => branchId.Value;
    public static implicit operator BranchId(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
