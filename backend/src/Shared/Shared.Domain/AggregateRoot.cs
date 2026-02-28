namespace Shared.Domain;

/// <summary>
/// Base class for aggregate roots. Extends Entity with BranchId for multi-tenant isolation
/// and domain event support. All aggregate roots must have a BranchId.
/// </summary>
public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public BranchId BranchId { get; private set; }

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot() : base() { }

    protected AggregateRoot(Guid id) : base(id) { }

    protected void SetBranchId(BranchId branchId)
    {
        BranchId = branchId;
    }

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
