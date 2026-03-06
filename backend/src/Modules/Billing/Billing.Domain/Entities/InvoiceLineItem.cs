using Billing.Domain.Enums;
using Shared.Domain;

namespace Billing.Domain.Entities;

/// <summary>
/// Child entity of Invoice. Represents a single billable item on an invoice.
/// Tracks department for revenue allocation per FIN-02.
/// SourceId/SourceType link back to the originating record (dispensing, visit service, etc.).
/// </summary>
public class InvoiceLineItem : Entity
{
    public Guid InvoiceId { get; private set; }

    /// <summary>English description of the line item.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Vietnamese description of the line item.</summary>
    public string? DescriptionVi { get; private set; }

    /// <summary>Unit price in VND.</summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>Quantity of units.</summary>
    public int Quantity { get; private set; }

    /// <summary>Department for revenue allocation.</summary>
    public Department Department { get; private set; }

    /// <summary>Computed line total: UnitPrice * Quantity.</summary>
    public decimal LineTotal => UnitPrice * Quantity;

    /// <summary>References the originating record (dispensing record, visit service, optical order, etc.).</summary>
    public Guid? SourceId { get; private set; }

    /// <summary>Type of the originating record ("Dispensing", "VisitService", "OpticalOrder", etc.).</summary>
    public string? SourceType { get; private set; }

    /// <summary>Private constructor for EF Core materialization.</summary>
    private InvoiceLineItem() { }

    /// <summary>
    /// Factory method for creating a new invoice line item.
    /// </summary>
    public static InvoiceLineItem Create(
        Guid invoiceId,
        string description,
        string? descriptionVi,
        decimal unitPrice,
        int quantity,
        Department department,
        Guid? sourceId = null,
        string? sourceType = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

        return new InvoiceLineItem
        {
            InvoiceId = invoiceId,
            Description = description,
            DescriptionVi = descriptionVi,
            UnitPrice = unitPrice,
            Quantity = quantity,
            Department = department,
            SourceId = sourceId,
            SourceType = sourceType
        };
    }
}
