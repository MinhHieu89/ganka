using FluentAssertions;
using NSubstitute;
using Optical.Application.Features.Alerts;
using Optical.Application.Features.Warranty;
using Optical.Application.Interfaces;
using Optical.Domain.Entities;
using Optical.Domain.Enums;
using Shared.Application.Services;
using Shared.Domain;

namespace Optical.Unit.Tests.Features;

/// <summary>
/// TDD tests for low lens stock alerts and warranty document upload:
///   - GetLowLensStockAlertsHandler (OPT-02)
///   - UploadWarrantyDocumentHandler
/// Follows the Wolverine static handler pattern established in project tests.
/// </summary>
public class AlertAndWarrantyDocumentHandlerTests
{
    private readonly ILensCatalogRepository _lensRepo = Substitute.For<ILensCatalogRepository>();
    private readonly IWarrantyClaimRepository _warrantyRepo = Substitute.For<IWarrantyClaimRepository>();
    private readonly IAzureBlobService _blobService = Substitute.For<IAzureBlobService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private static readonly BranchId TestBranchId = new(Guid.NewGuid());

    private static LensCatalogItem CreateCatalogItem(
        string brand = "Essilor",
        string name = "Crizal SV",
        string lensType = "single_vision") =>
        LensCatalogItem.Create(brand, name, lensType, LensMaterial.CR39, LensCoating.AntiReflective,
            1_200_000m, 600_000m, null, TestBranchId);

    // ─── GetLowLensStockAlerts Tests ─────────────────────────────────────────

    [Fact]
    public async Task GetLowLensStockAlerts_ItemsBelowMinStock_ReturnsAlerts()
    {
        // Arrange
        var item = CreateCatalogItem(brand: "Essilor", name: "Crizal SV");
        item.AddStockEntry(sph: -2.00m, cyl: 0m, add: null, quantity: 1); // below default MinStockLevel of 2
        item.AddStockEntry(sph: -1.00m, cyl: 0m, add: null, quantity: 5); // above MinStockLevel

        _lensRepo.GetAllAsync(false, Arg.Any<CancellationToken>())
            .Returns(new List<LensCatalogItem> { item });

        var query = new GetLowLensStockAlertsQuery();

        // Act
        var result = await GetLowLensStockAlertsHandler.Handle(query, _lensRepo, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        var alert = result.Value[0];
        alert.LensCatalogItemId.Should().Be(item.Id);
        alert.LensName.Should().Be("Crizal SV");
        alert.Brand.Should().Be("Essilor");
        alert.Sph.Should().Be(-2.00m);
        alert.Cyl.Should().Be(0m);
        alert.CurrentStock.Should().Be(1);
        alert.MinStockLevel.Should().Be(2);
    }

    [Fact]
    public async Task GetLowLensStockAlerts_AllItemsAboveMinStock_ReturnsEmptyList()
    {
        // Arrange
        var item = CreateCatalogItem();
        item.AddStockEntry(sph: -2.00m, cyl: 0m, add: null, quantity: 10);

        _lensRepo.GetAllAsync(false, Arg.Any<CancellationToken>())
            .Returns(new List<LensCatalogItem> { item });

        var query = new GetLowLensStockAlertsQuery();

        // Act
        var result = await GetLowLensStockAlertsHandler.Handle(query, _lensRepo, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLowLensStockAlerts_NoItems_ReturnsEmptyList()
    {
        // Arrange
        _lensRepo.GetAllAsync(false, Arg.Any<CancellationToken>())
            .Returns(new List<LensCatalogItem>());

        var query = new GetLowLensStockAlertsQuery();

        // Act
        var result = await GetLowLensStockAlertsHandler.Handle(query, _lensRepo, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLowLensStockAlerts_MultipleItemsWithLowStock_ReturnsAllAlerts()
    {
        // Arrange
        var item1 = CreateCatalogItem(brand: "Essilor", name: "Lens A");
        item1.AddStockEntry(sph: -1.00m, cyl: 0m, add: null, quantity: 0); // below threshold

        var item2 = CreateCatalogItem(brand: "Hoya", name: "Lens B");
        item2.AddStockEntry(sph: -2.00m, cyl: -0.50m, add: null, quantity: 1); // below threshold

        _lensRepo.GetAllAsync(false, Arg.Any<CancellationToken>())
            .Returns(new List<LensCatalogItem> { item1, item2 });

        var query = new GetLowLensStockAlertsQuery();

        // Act
        var result = await GetLowLensStockAlertsHandler.Handle(query, _lensRepo, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetLowLensStockAlerts_IncludesAddPowerField()
    {
        // Arrange
        var item = CreateCatalogItem(lensType: "progressive");
        item.AddStockEntry(sph: -1.00m, cyl: 0m, add: 2.00m, quantity: 1); // progressive with add

        _lensRepo.GetAllAsync(false, Arg.Any<CancellationToken>())
            .Returns(new List<LensCatalogItem> { item });

        var query = new GetLowLensStockAlertsQuery();

        // Act
        var result = await GetLowLensStockAlertsHandler.Handle(query, _lensRepo, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Add.Should().Be(2.00m);
    }

    // ─── UploadWarrantyDocument Tests ─────────────────────────────────────────

    [Fact]
    public async Task UploadWarrantyDocument_ValidClaim_UploadsAndAddsUrl()
    {
        // Arrange
        var claim = WarrantyClaim.Create(
            glassesOrderId: Guid.NewGuid(),
            claimDate: DateTime.UtcNow,
            resolution: WarrantyResolution.Repair,
            assessmentNotes: "Hinge broken",
            discountAmount: null);

        _warrantyRepo.GetByIdAsync(claim.Id, Arg.Any<CancellationToken>())
            .Returns(claim);

        using var stream = new MemoryStream(new byte[1024]);
        const string expectedUrl = "https://blob.storage/warranty-documents/claim-doc.pdf";
        _blobService.UploadAsync(
                Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<Stream>(), Arg.Any<string>())
            .Returns(expectedUrl);

        var command = new UploadWarrantyDocumentCommand(claim.Id, stream, "claim-doc.pdf");

        // Act
        var result = await UploadWarrantyDocumentHandler.Handle(
            command, _warrantyRepo, _blobService, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedUrl);
        claim.DocumentUrls.Should().Contain(expectedUrl);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UploadWarrantyDocument_ClaimNotFound_ReturnsNotFound()
    {
        // Arrange
        _warrantyRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((WarrantyClaim?)null);

        using var stream = new MemoryStream();
        var command = new UploadWarrantyDocumentCommand(Guid.NewGuid(), stream, "doc.pdf");

        // Act
        var result = await UploadWarrantyDocumentHandler.Handle(
            command, _warrantyRepo, _blobService, _unitOfWork, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
        await _blobService.DidNotReceive().UploadAsync(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<Stream>(), Arg.Any<string>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UploadWarrantyDocument_UploadsToWarrantyDocumentsContainer()
    {
        // Arrange
        var claim = WarrantyClaim.Create(
            glassesOrderId: Guid.NewGuid(),
            claimDate: DateTime.UtcNow,
            resolution: WarrantyResolution.Replace,
            assessmentNotes: "Lens cracked",
            discountAmount: null);

        _warrantyRepo.GetByIdAsync(claim.Id, Arg.Any<CancellationToken>())
            .Returns(claim);

        using var stream = new MemoryStream(new byte[512]);
        _blobService.UploadAsync(
                Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<Stream>(), Arg.Any<string>())
            .Returns("https://blob.storage/warranty-documents/photo.jpg");

        var command = new UploadWarrantyDocumentCommand(claim.Id, stream, "photo.jpg");

        // Act
        await UploadWarrantyDocumentHandler.Handle(
            command, _warrantyRepo, _blobService, _unitOfWork, CancellationToken.None);

        // Assert - uploaded to the correct container
        await _blobService.Received(1).UploadAsync(
            "warranty-documents",
            Arg.Any<string>(),
            stream,
            Arg.Any<string>());
    }

    [Fact]
    public async Task UploadWarrantyDocument_MultipleDocs_AddsAllUrlsToClaim()
    {
        // Arrange
        var claim = WarrantyClaim.Create(
            glassesOrderId: Guid.NewGuid(),
            claimDate: DateTime.UtcNow,
            resolution: WarrantyResolution.Replace,
            assessmentNotes: "Frame damaged",
            discountAmount: null);

        _warrantyRepo.GetByIdAsync(claim.Id, Arg.Any<CancellationToken>())
            .Returns(claim);

        var url1 = "https://blob.storage/warranty-documents/doc1.pdf";
        var url2 = "https://blob.storage/warranty-documents/doc2.jpg";

        _blobService.UploadAsync(Arg.Any<string>(), Arg.Is<string>(s => s.Contains("doc1.pdf")),
                Arg.Any<Stream>(), Arg.Any<string>())
            .Returns(url1);
        _blobService.UploadAsync(Arg.Any<string>(), Arg.Is<string>(s => s.Contains("doc2.jpg")),
                Arg.Any<Stream>(), Arg.Any<string>())
            .Returns(url2);

        using var stream1 = new MemoryStream(new byte[100]);
        using var stream2 = new MemoryStream(new byte[200]);

        // Act - upload two documents
        await UploadWarrantyDocumentHandler.Handle(
            new UploadWarrantyDocumentCommand(claim.Id, stream1, "doc1.pdf"),
            _warrantyRepo, _blobService, _unitOfWork, CancellationToken.None);
        await UploadWarrantyDocumentHandler.Handle(
            new UploadWarrantyDocumentCommand(claim.Id, stream2, "doc2.jpg"),
            _warrantyRepo, _blobService, _unitOfWork, CancellationToken.None);

        // Assert - both URLs added to claim
        claim.DocumentUrls.Should().HaveCount(2);
        claim.DocumentUrls.Should().Contain(url1);
        claim.DocumentUrls.Should().Contain(url2);
    }
}
