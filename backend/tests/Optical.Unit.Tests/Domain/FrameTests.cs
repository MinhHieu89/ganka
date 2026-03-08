using FluentAssertions;
using Optical.Domain.Entities;
using Optical.Domain.Enums;
using Shared.Domain;

namespace Optical.Unit.Tests.Domain;

/// <summary>
/// Domain tests for Frame entity: creation, stock adjustments, and field updates.
/// </summary>
public class FrameTests
{
    private static readonly BranchId TestBranchId = new(Guid.NewGuid());

    private static Frame CreateFrame(
        string brand = "Ray-Ban",
        string model = "RB3025",
        string color = "Matte Black",
        int lensWidth = 52,
        int bridgeWidth = 18,
        int templeLength = 140,
        FrameMaterial material = FrameMaterial.Metal,
        FrameType type = FrameType.FullRim,
        FrameGender gender = FrameGender.Unisex,
        decimal sellingPrice = 2_500_000m,
        decimal costPrice = 1_200_000m,
        string? barcode = null,
        BranchId? branchId = null) =>
        Frame.Create(
            brand, model, color,
            lensWidth, bridgeWidth, templeLength,
            material, type, gender,
            sellingPrice, costPrice, barcode,
            branchId ?? TestBranchId);

    // --- Create Tests ---

    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        // Arrange & Act
        var frame = CreateFrame(
            brand: "Ray-Ban",
            model: "RB3025",
            color: "Matte Black",
            lensWidth: 52,
            bridgeWidth: 18,
            templeLength: 140,
            material: FrameMaterial.Metal,
            type: FrameType.FullRim,
            gender: FrameGender.Unisex,
            sellingPrice: 2_500_000m,
            costPrice: 1_200_000m,
            barcode: "5901234123457");

        // Assert
        frame.Brand.Should().Be("Ray-Ban");
        frame.Model.Should().Be("RB3025");
        frame.Color.Should().Be("Matte Black");
        frame.LensWidth.Should().Be(52);
        frame.BridgeWidth.Should().Be(18);
        frame.TempleLength.Should().Be(140);
        frame.Material.Should().Be(FrameMaterial.Metal);
        frame.Type.Should().Be(FrameType.FullRim);
        frame.Gender.Should().Be(FrameGender.Unisex);
        frame.SellingPrice.Should().Be(2_500_000m);
        frame.CostPrice.Should().Be(1_200_000m);
        frame.Barcode.Should().Be("5901234123457");
        frame.StockQuantity.Should().Be(0);
        frame.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldSetBranchId()
    {
        // Arrange
        var branchId = new BranchId(Guid.NewGuid());

        // Act
        var frame = CreateFrame(branchId: branchId);

        // Assert
        frame.BranchId.Should().Be(branchId);
    }

    [Fact]
    public void Create_ShouldInitializeStockQuantityToZero()
    {
        // Act
        var frame = CreateFrame();

        // Assert
        frame.StockQuantity.Should().Be(0);
    }

    [Fact]
    public void Create_ShouldInitializeIsActiveToTrue()
    {
        // Act
        var frame = CreateFrame();

        // Assert
        frame.IsActive.Should().BeTrue();
    }

    // --- AdjustStock Tests ---

    [Fact]
    public void AdjustStock_ShouldIncreaseQuantity()
    {
        // Arrange
        var frame = CreateFrame();

        // Act
        frame.AdjustStock(10);

        // Assert
        frame.StockQuantity.Should().Be(10);
    }

    [Fact]
    public void AdjustStock_ShouldDecreaseQuantity()
    {
        // Arrange
        var frame = CreateFrame();
        frame.AdjustStock(20);

        // Act
        frame.AdjustStock(-5);

        // Assert
        frame.StockQuantity.Should().Be(15);
    }

    [Fact]
    public void AdjustStock_NegativeResult_ShouldThrow()
    {
        // Arrange
        var frame = CreateFrame();
        frame.AdjustStock(5);

        // Act
        var act = () => frame.AdjustStock(-10);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot reduce stock below zero*");
    }

    [Fact]
    public void AdjustStock_ToZero_ShouldNotThrow()
    {
        // Arrange
        var frame = CreateFrame();
        frame.AdjustStock(5);

        // Act
        var act = () => frame.AdjustStock(-5);

        // Assert
        act.Should().NotThrow();
        frame.StockQuantity.Should().Be(0);
    }

    // --- Update Tests ---

    [Fact]
    public void Update_ShouldModifyFields()
    {
        // Arrange
        var frame = CreateFrame();

        // Act
        frame.Update(
            brand: "Oakley",
            model: "OX8156",
            color: "Satin Black",
            lensWidth: 55,
            bridgeWidth: 17,
            templeLength: 145,
            material: FrameMaterial.Titanium,
            type: FrameType.SemiRimless,
            gender: FrameGender.Male,
            sellingPrice: 3_000_000m,
            costPrice: 1_500_000m,
            barcode: "4006381333931");

        // Assert
        frame.Brand.Should().Be("Oakley");
        frame.Model.Should().Be("OX8156");
        frame.Color.Should().Be("Satin Black");
        frame.LensWidth.Should().Be(55);
        frame.BridgeWidth.Should().Be(17);
        frame.TempleLength.Should().Be(145);
        frame.Material.Should().Be(FrameMaterial.Titanium);
        frame.Type.Should().Be(FrameType.SemiRimless);
        frame.Gender.Should().Be(FrameGender.Male);
        frame.SellingPrice.Should().Be(3_000_000m);
        frame.CostPrice.Should().Be(1_500_000m);
        frame.Barcode.Should().Be("4006381333931");
    }

    [Fact]
    public void SizeDisplay_ShouldReturnOpticalNotationFormat()
    {
        // Arrange
        var frame = CreateFrame(lensWidth: 52, bridgeWidth: 18, templeLength: 140);

        // Assert
        frame.SizeDisplay.Should().Be("52-18-140");
    }
}
