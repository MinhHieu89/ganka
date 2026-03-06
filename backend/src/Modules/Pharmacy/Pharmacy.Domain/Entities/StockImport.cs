using Pharmacy.Domain.Enums;
using Shared.Domain;

namespace Pharmacy.Domain.Entities;

/// <summary>
/// AggregateRoot for a stock import event — tracks a single supplier invoice or Excel bulk import.
/// Each StockImport contains one or more StockImportLine children, one per drug batch received.
/// Provides a full audit trail: who imported, when, from which supplier, against which invoice.
/// </summary>
public class StockImport : AggregateRoot, IAuditable
{
    /// <summary>Foreign key to the Supplier who provided the stock.</summary>
    public Guid SupplierId { get; private set; }

    /// <summary>
    /// Denormalized supplier name for display without a join.
    /// Snapshot at import time in case the supplier name changes later.
    /// </summary>
    public string SupplierName { get; private set; } = string.Empty;

    /// <summary>Whether this import was entered via supplier invoice form or Excel bulk upload.</summary>
    public ImportSource ImportSource { get; private set; }

    /// <summary>Supplier's invoice number for reconciliation (optional).</summary>
    public string? InvoiceNumber { get; private set; }

    /// <summary>Foreign key to the user (pharmacist) who performed the import.</summary>
    public Guid ImportedById { get; private set; }

    /// <summary>UTC timestamp when the import was recorded in the system.</summary>
    public DateTime ImportedAt { get; private set; }

    /// <summary>Optional notes about the import (e.g., discrepancies, special conditions).</summary>
    public string? Notes { get; private set; }

    private readonly List<StockImportLine> _lines = [];

    /// <summary>The drug lines included in this import (one per drug batch received).</summary>
    public IReadOnlyCollection<StockImportLine> Lines => _lines.AsReadOnly();

    /// <summary>Private constructor for EF Core materialization.</summary>
    private StockImport() { }

    /// <summary>
    /// Factory method for creating a new stock import transaction.
    /// </summary>
    /// <param name="supplierId">The supplier providing the stock.</param>
    /// <param name="supplierName">Denormalized supplier name (snapshot at import time).</param>
    /// <param name="importSource">Whether entered via invoice form or Excel upload.</param>
    /// <param name="invoiceNumber">Supplier invoice number for reconciliation (optional).</param>
    /// <param name="importedById">The user (pharmacist) performing the import.</param>
    /// <param name="notes">Optional notes about the import.</param>
    /// <param name="branchId">The branch this import belongs to.</param>
    public static StockImport Create(
        Guid supplierId,
        string supplierName,
        ImportSource importSource,
        string? invoiceNumber,
        Guid importedById,
        string? notes,
        BranchId branchId)
    {
        if (string.IsNullOrWhiteSpace(supplierName))
            throw new ArgumentException("Supplier name is required.", nameof(supplierName));

        var import = new StockImport
        {
            SupplierId = supplierId,
            SupplierName = supplierName,
            ImportSource = importSource,
            InvoiceNumber = invoiceNumber,
            ImportedById = importedById,
            ImportedAt = DateTime.UtcNow,
            Notes = notes
        };

        import.SetBranchId(branchId);
        return import;
    }

    /// <summary>
    /// Adds a drug line to this import transaction.
    /// Creates and tracks a StockImportLine for one drug batch received.
    /// </summary>
    /// <param name="drugCatalogItemId">The drug catalog entry for the drug received.</param>
    /// <param name="drugName">Denormalized drug name (snapshot at import time).</param>
    /// <param name="batchNumber">Manufacturer batch number for traceability.</param>
    /// <param name="expiryDate">Expiry date of this batch (must be in the future).</param>
    /// <param name="quantity">Quantity received (must be positive).</param>
    /// <param name="purchasePrice">Purchase price per unit in VND (must be non-negative).</param>
    public StockImportLine AddLine(
        Guid drugCatalogItemId,
        string drugName,
        string batchNumber,
        DateOnly expiryDate,
        int quantity,
        decimal purchasePrice)
    {
        var line = StockImportLine.Create(Id, drugCatalogItemId, drugName, batchNumber, expiryDate, quantity, purchasePrice);
        _lines.Add(line);
        SetUpdatedAt();
        return line;
    }
}
