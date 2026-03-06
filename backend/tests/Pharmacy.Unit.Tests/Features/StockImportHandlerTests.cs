using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Pharmacy.Application.Features.StockImport;
using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;
using Pharmacy.Domain.Entities;
using Pharmacy.Domain.Enums;
using Shared.Application;
using Shared.Domain;

namespace Pharmacy.Unit.Tests.Features;

/// <summary>
/// TDD tests for Stock Import handlers: CreateStockImport, ImportStockFromExcel, GetStockImports.
/// PHR-02: Staff can import stock via supplier invoice or Excel bulk import.
/// </summary>
public class StockImportHandlerTests
{
    private readonly IStockImportRepository _stockImportRepository = Substitute.For<IStockImportRepository>();
    private readonly IDrugBatchRepository _drugBatchRepository = Substitute.For<IDrugBatchRepository>();
    private readonly ISupplierRepository _supplierRepository = Substitute.For<ISupplierRepository>();
    private readonly IDrugCatalogItemRepository _drugCatalogItemRepository = Substitute.For<IDrugCatalogItemRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IValidator<CreateStockImportCommand> _createValidator = Substitute.For<IValidator<CreateStockImportCommand>>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private static readonly Guid DefaultBranchId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid DefaultSupplierId = Guid.NewGuid();
    private static readonly Guid DefaultDrugId = Guid.NewGuid();

    public StockImportHandlerTests()
    {
        _currentUser.BranchId.Returns(DefaultBranchId);
        _currentUser.UserId.Returns(Guid.NewGuid());
    }

    private void SetupValidCreateValidator()
    {
        _createValidator.ValidateAsync(Arg.Any<CreateStockImportCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private Supplier CreateTestSupplier() =>
        Supplier.Create(
            "Công ty Dược Phẩm Sài Gòn",
            "123 Nguyễn Huệ, Q.1, TP.HCM",
            "0901234567",
            "info@saigonpharma.vn",
            new BranchId(DefaultBranchId));

    private DrugCatalogItem CreateTestDrugItem() =>
        DrugCatalogItem.Create(
            "Tobramycin 0.3%",
            "Tobramycin 0,3%",
            "Tobramycin",
            DrugForm.EyeDrops,
            "0.3%",
            DrugRoute.Topical,
            "Chai",
            "1-2 drops x 4 times/day",
            new BranchId(DefaultBranchId));

    private static StockImportLineInput CreateValidLineInput(Guid? drugCatalogItemId = null) =>
        new(
            DrugCatalogItemId: drugCatalogItemId ?? DefaultDrugId,
            DrugName: "Tobramycin 0.3%",
            BatchNumber: "BN2026001",
            ExpiryDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2)),
            Quantity: 100,
            PurchasePrice: 50000m);

    private static CreateStockImportCommand CreateValidCommand() =>
        new(
            SupplierId: DefaultSupplierId,
            InvoiceNumber: "INV-2026-001",
            Notes: "Nhập hàng tháng 3/2026",
            Lines: new List<StockImportLineInput>
            {
                CreateValidLineInput()
            });

    #region CreateStockImport Tests

    [Fact]
    public async Task CreateStockImport_ValidInvoice_CreatesStockImportAndBatches()
    {
        // Arrange
        SetupValidCreateValidator();
        var supplier = CreateTestSupplier();
        var drug = CreateTestDrugItem();
        var command = CreateValidCommand();

        _supplierRepository.GetByIdAsync(command.SupplierId, Arg.Any<CancellationToken>())
            .Returns(supplier);
        _drugCatalogItemRepository.GetByIdAsync(command.Lines[0].DrugCatalogItemId, Arg.Any<CancellationToken>())
            .Returns(drug);

        // Act
        var result = await CreateStockImportHandler.Handle(
            command, _stockImportRepository, _drugBatchRepository, _supplierRepository,
            _drugCatalogItemRepository, _unitOfWork, _createValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _stockImportRepository.Received(1).Add(Arg.Any<StockImport>());
        _drugBatchRepository.Received(1).Add(Arg.Any<DrugBatch>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateStockImport_InvalidData_ReturnsValidationErrors()
    {
        // Arrange
        var command = CreateValidCommand() with { SupplierId = Guid.Empty };
        _createValidator.ValidateAsync(Arg.Any<CreateStockImportCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("SupplierId", "Supplier is required.")
            }));

        // Act
        var result = await CreateStockImportHandler.Handle(
            command, _stockImportRepository, _drugBatchRepository, _supplierRepository,
            _drugCatalogItemRepository, _unitOfWork, _createValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
        _stockImportRepository.DidNotReceive().Add(Arg.Any<StockImport>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateStockImport_FutureExpiryRequired_RejectsExpiredDates()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Lines = new List<StockImportLineInput>
            {
                CreateValidLineInput() with
                {
                    ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))
                }
            }
        };
        _createValidator.ValidateAsync(Arg.Any<CreateStockImportCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("Lines[0].ExpiryDate", "Expiry date must be in the future.")
            }));

        // Act
        var result = await CreateStockImportHandler.Handle(
            command, _stockImportRepository, _drugBatchRepository, _supplierRepository,
            _drugCatalogItemRepository, _unitOfWork, _createValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task CreateStockImport_SupplierNotFound_ReturnsNotFoundError()
    {
        // Arrange
        SetupValidCreateValidator();
        var command = CreateValidCommand();

        _supplierRepository.GetByIdAsync(command.SupplierId, Arg.Any<CancellationToken>())
            .Returns((Supplier?)null);

        // Act
        var result = await CreateStockImportHandler.Handle(
            command, _stockImportRepository, _drugBatchRepository, _supplierRepository,
            _drugCatalogItemRepository, _unitOfWork, _createValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task CreateStockImport_MultipleLines_CreatesMultipleBatches()
    {
        // Arrange
        SetupValidCreateValidator();
        var supplier = CreateTestSupplier();
        var drug1 = CreateTestDrugItem();
        var drug2 = CreateTestDrugItem();
        var drugId2 = Guid.NewGuid();

        var command = new CreateStockImportCommand(
            SupplierId: DefaultSupplierId,
            InvoiceNumber: "INV-2026-002",
            Notes: null,
            Lines: new List<StockImportLineInput>
            {
                CreateValidLineInput(DefaultDrugId),
                new(
                    DrugCatalogItemId: drugId2,
                    DrugName: "Cyclosporine 0.05%",
                    BatchNumber: "BN2026002",
                    ExpiryDate: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
                    Quantity: 50,
                    PurchasePrice: 120000m)
            });

        _supplierRepository.GetByIdAsync(command.SupplierId, Arg.Any<CancellationToken>())
            .Returns(supplier);
        _drugCatalogItemRepository.GetByIdAsync(DefaultDrugId, Arg.Any<CancellationToken>())
            .Returns(drug1);
        _drugCatalogItemRepository.GetByIdAsync(drugId2, Arg.Any<CancellationToken>())
            .Returns(drug2);

        // Act
        var result = await CreateStockImportHandler.Handle(
            command, _stockImportRepository, _drugBatchRepository, _supplierRepository,
            _drugCatalogItemRepository, _unitOfWork, _createValidator, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _drugBatchRepository.Received(2).Add(Arg.Any<DrugBatch>());
    }

    #endregion

    #region ImportStockFromExcel Tests

    [Fact]
    public async Task ImportStockFromExcel_ValidData_ReturnsPreviewLines()
    {
        // Arrange
        // Create a minimal valid Excel stream with the expected columns
        var excelData = CreateValidExcelStream();
        var drug = CreateTestDrugItem();

        _drugCatalogItemRepository.SearchAsync("Tobramycin 0.3%", Arg.Any<CancellationToken>())
            .Returns(new List<DrugCatalogItemDto>
            {
                new(
                    Id: DefaultDrugId,
                    Name: "Tobramycin 0.3%",
                    NameVi: "Tobramycin 0,3%",
                    GenericName: "Tobramycin",
                    Form: (int)DrugForm.EyeDrops,
                    Strength: "0.3%",
                    Route: (int)DrugRoute.Topical,
                    Unit: "Chai",
                    DefaultDosageTemplate: null,
                    IsActive: true,
                    SellingPrice: 75000m,
                    MinStockLevel: 10)
            });

        var command = new ImportStockFromExcelCommand(excelData, DefaultSupplierId);

        // Act
        var result = await ImportStockFromExcelHandler.Handle(
            command, _drugCatalogItemRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ValidLines.Should().NotBeEmpty();
        result.Value.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ImportStockFromExcel_InvalidRows_CollectsAllErrors()
    {
        // Arrange - create an Excel stream with invalid rows (negative qty, expired date)
        var excelData = CreateInvalidExcelStream();

        var command = new ImportStockFromExcelCommand(excelData, DefaultSupplierId);

        // Act
        var result = await ImportStockFromExcelHandler.Handle(
            command, _drugCatalogItemRepository, CancellationToken.None);

        // Assert - should succeed but with errors in preview, not fail-fast
        result.IsSuccess.Should().BeTrue();
        result.Value.Errors.Should().NotBeEmpty();
    }

    #endregion

    #region GetStockImports Tests

    [Fact]
    public async Task GetStockImports_ReturnsPaginated()
    {
        // Arrange
        var items = new List<StockImportDto>
        {
            new(
                Id: Guid.NewGuid(),
                SupplierId: DefaultSupplierId,
                SupplierName: "Công ty Dược Phẩm ABC",
                ImportSource: (int)ImportSource.SupplierInvoice,
                InvoiceNumber: "INV-001",
                ImportedAt: DateTime.UtcNow.AddDays(-1),
                Notes: null,
                Lines: new List<StockImportLineDto>())
        };

        _stockImportRepository.GetAllAsync(1, 20, Arg.Any<CancellationToken>())
            .Returns((items, 1));

        var query = new GetStockImportsQuery(Page: 1, PageSize: 20);

        // Act
        var result = await GetStockImportsHandler.Handle(query, _stockImportRepository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(1);
        result.Value.Items[0].SupplierName.Should().Be("Công ty Dược Phẩm ABC");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a minimal valid Excel stream using MiniExcel format.
    /// This is a pre-built valid .xlsx byte array for testing purposes.
    /// We use a simple in-memory approach for testing.
    /// </summary>
    private static Stream CreateValidExcelStream()
    {
        // We create the Excel in memory using MiniExcel save API
        // Row: DrugName, BatchNumber, ExpiryDate, Quantity, PurchasePrice
        var rows = new[]
        {
            new Dictionary<string, object>
            {
                ["DrugName"] = "Tobramycin 0.3%",
                ["BatchNumber"] = "BN2026001",
                ["ExpiryDate"] = DateTime.UtcNow.AddYears(2).ToString("yyyy-MM-dd"),
                ["Quantity"] = 100,
                ["PurchasePrice"] = 50000
            }
        };

        var stream = new MemoryStream();
        MiniExcelLibs.MiniExcel.SaveAs(stream, rows);
        stream.Position = 0;
        return stream;
    }

    /// <summary>
    /// Creates an Excel stream with invalid data rows for error collection testing.
    /// Rows contain: negative quantity and past expiry date.
    /// </summary>
    private static Stream CreateInvalidExcelStream()
    {
        var rows = new[]
        {
            new Dictionary<string, object>
            {
                ["DrugName"] = "UnknownDrug",
                ["BatchNumber"] = "",           // Missing batch number
                ["ExpiryDate"] = DateTime.UtcNow.AddDays(-10).ToString("yyyy-MM-dd"), // Past date
                ["Quantity"] = -5,              // Negative quantity
                ["PurchasePrice"] = 50000
            }
        };

        var stream = new MemoryStream();
        MiniExcelLibs.MiniExcel.SaveAs(stream, rows);
        stream.Position = 0;
        return stream;
    }

    #endregion
}
