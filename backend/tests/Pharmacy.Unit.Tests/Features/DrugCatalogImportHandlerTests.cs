using FluentAssertions;
using NSubstitute;
using Pharmacy.Application.Features.DrugCatalog;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Domain.Enums;
using MiniExcelLibs;
using Shared.Domain;

namespace Pharmacy.Unit.Tests.Features;

public class DrugCatalogImportHandlerTests
{
    private readonly IDrugCatalogItemRepository _repository = Substitute.For<IDrugCatalogItemRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private static readonly Guid DefaultBranchId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    #region ImportDrugCatalogFromExcel Tests

    [Fact]
    public async Task Import_WithValidRows_ReturnsPreviewWithValidRows()
    {
        // Arrange
        var rows = new List<Dictionary<string, object>>
        {
            new()
            {
                ["Name"] = "Tobramycin 0.3%",
                ["NameVi"] = "Tobramycin 0,3%",
                ["GenericName"] = "Tobramycin",
                ["Form"] = "EyeDrops",
                ["Route"] = "Topical",
                ["Strength"] = "0.3%",
                ["Unit"] = "Chai",
                ["SellingPrice"] = 50000,
                ["MinStockLevel"] = 10
            }
        };
        var stream = CreateExcelStream(rows);
        var command = new ImportDrugCatalogFromExcelCommand(stream, "test.xlsx");

        // Act
        var result = await ImportDrugCatalogFromExcelHandler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ValidRows.Should().HaveCount(1);
        result.Value.Errors.Should().BeEmpty();
        result.Value.ValidRows[0].Name.Should().Be("Tobramycin 0.3%");
    }

    [Fact]
    public async Task Import_WithMissingName_ReturnsErrorOnNameColumn()
    {
        // Arrange
        var rows = new List<Dictionary<string, object>>
        {
            new()
            {
                ["Name"] = "",
                ["NameVi"] = "Thuoc",
                ["GenericName"] = "Generic",
                ["Form"] = "Tablet",
                ["Route"] = "Oral",
                ["Strength"] = "500mg",
                ["Unit"] = "Vien",
                ["SellingPrice"] = 10000,
                ["MinStockLevel"] = 5
            }
        };
        var stream = CreateExcelStream(rows);
        var command = new ImportDrugCatalogFromExcelCommand(stream, "test.xlsx");

        // Act
        var result = await ImportDrugCatalogFromExcelHandler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ValidRows.Should().BeEmpty();
        result.Value.Errors.Should().ContainSingle(e =>
            e.ColumnName == "Name" && e.Message.Contains("required"));
    }

    [Fact]
    public async Task Import_WithNegativeSellingPrice_ReturnsErrorOnSellingPriceColumn()
    {
        // Arrange
        var rows = new List<Dictionary<string, object>>
        {
            new()
            {
                ["Name"] = "TestDrug",
                ["NameVi"] = "Thuoc",
                ["GenericName"] = "Generic",
                ["Form"] = "Tablet",
                ["Route"] = "Oral",
                ["Strength"] = "500mg",
                ["Unit"] = "Vien",
                ["SellingPrice"] = -100,
                ["MinStockLevel"] = 5
            }
        };
        var stream = CreateExcelStream(rows);
        var command = new ImportDrugCatalogFromExcelCommand(stream, "test.xlsx");

        // Act
        var result = await ImportDrugCatalogFromExcelHandler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ValidRows.Should().BeEmpty();
        result.Value.Errors.Should().ContainSingle(e =>
            e.ColumnName == "SellingPrice" && e.Message.Contains("positive"));
    }

    [Fact]
    public async Task Import_WithMixedValidAndInvalidRows_ReturnsBoth()
    {
        // Arrange
        var rows = new List<Dictionary<string, object>>
        {
            new()
            {
                ["Name"] = "ValidDrug",
                ["NameVi"] = "Thuoc",
                ["GenericName"] = "Generic",
                ["Form"] = "Tablet",
                ["Route"] = "Oral",
                ["Strength"] = "10mg",
                ["Unit"] = "Vien",
                ["SellingPrice"] = 5000,
                ["MinStockLevel"] = 0
            },
            new()
            {
                ["Name"] = "",
                ["NameVi"] = "Thuoc2",
                ["GenericName"] = "Generic2",
                ["Form"] = "Tablet",
                ["Route"] = "Oral",
                ["Strength"] = "20mg",
                ["Unit"] = "Vien",
                ["SellingPrice"] = 3000,
                ["MinStockLevel"] = 0
            }
        };
        var stream = CreateExcelStream(rows);
        var command = new ImportDrugCatalogFromExcelCommand(stream, "test.xlsx");

        // Act
        var result = await ImportDrugCatalogFromExcelHandler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ValidRows.Should().HaveCount(1);
        result.Value.Errors.Should().HaveCount(1);
    }

    [Fact]
    public async Task Import_WithEmptyFile_ReturnsValidationError()
    {
        // Arrange
        var stream = CreateExcelStream(new List<Dictionary<string, object>>());
        var command = new ImportDrugCatalogFromExcelCommand(stream, "test.xlsx");

        // Act
        var result = await ImportDrugCatalogFromExcelHandler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region ConfirmDrugCatalogImport Tests

    [Fact]
    public async Task Confirm_WithValidRows_CreatesItemsAndReturnsCount()
    {
        // Arrange
        var validRows = new List<ValidDrugCatalogRow>
        {
            new("Tobramycin 0.3%", "Tobramycin 0,3%", "Tobramycin",
                "EyeDrops", "Topical", "0.3%", "Chai", 50000m, 10),
            new("Amoxicillin 500mg", "Amoxicillin 500mg", "Amoxicillin",
                "Capsule", "Oral", "500mg", "Vien", 3000m, 20)
        };

        var command = new ConfirmDrugCatalogImportCommand(validRows, DefaultBranchId);

        // Act
        var result = await ConfirmDrugCatalogImportHandler.Handle(
            command, _repository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);
        _repository.Received(2).Add(Arg.Any<DrugCatalogItem>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Confirm_WithEmptyRows_ReturnsZero()
    {
        // Arrange
        var command = new ConfirmDrugCatalogImportCommand(new List<ValidDrugCatalogRow>(), DefaultBranchId);

        // Act
        var result = await ConfirmDrugCatalogImportHandler.Handle(
            command, _repository, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    #endregion

    #region GetDrugCatalogTemplate Tests

    [Fact]
    public void GetTemplate_ReturnsNonEmptyByteArray()
    {
        // Act
        var result = GetDrugCatalogTemplateHandler.Handle(new GetDrugCatalogTemplateQuery());

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetTemplate_ContainsExpectedColumns()
    {
        // Act
        var result = GetDrugCatalogTemplateHandler.Handle(new GetDrugCatalogTemplateQuery());

        // Assert - verify the template can be read back and has expected column headers
        using var stream = new MemoryStream(result);
        var rows = MiniExcel.Query(stream, useHeaderRow: true).ToList();
        rows.Should().HaveCount(1); // One template row with empty values

        var firstRow = (IDictionary<string, object>)rows[0];
        firstRow.Keys.Should().Contain("Name");
        firstRow.Keys.Should().Contain("NameVi");
        firstRow.Keys.Should().Contain("GenericName");
        firstRow.Keys.Should().Contain("Form");
        firstRow.Keys.Should().Contain("Route");
        firstRow.Keys.Should().Contain("Strength");
        firstRow.Keys.Should().Contain("Unit");
        firstRow.Keys.Should().Contain("SellingPrice");
        firstRow.Keys.Should().Contain("MinStockLevel");
    }

    #endregion

    private static Stream CreateExcelStream(List<Dictionary<string, object>> rows)
    {
        var stream = new MemoryStream();
        MiniExcel.SaveAs(stream, rows);
        stream.Position = 0;
        return stream;
    }
}
