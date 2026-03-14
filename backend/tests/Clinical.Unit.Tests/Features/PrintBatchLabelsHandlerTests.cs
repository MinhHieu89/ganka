using Clinical.Application.Features.Prescriptions;
using Clinical.Application.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace Clinical.Unit.Tests.Features;

public class PrintBatchLabelsHandlerTests
{
    private readonly IDocumentService _documentService = Substitute.For<IDocumentService>();

    [Fact]
    public async Task Handle_ValidPrescription_ReturnsPdfBytes()
    {
        // Arrange
        var prescriptionId = Guid.NewGuid();
        var query = new PrintBatchLabelsQuery(prescriptionId);

        _documentService.GenerateBatchPharmacyLabelsAsync(
            prescriptionId, Arg.Any<CancellationToken>())
            .Returns(new byte[] { 0x25, 0x50, 0x44, 0x46 }); // %PDF header

        // Act
        var result = await PrintBatchLabelsHandler.Handle(
            query, _documentService, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_PrescriptionNotFound_ReturnsError()
    {
        // Arrange
        var query = new PrintBatchLabelsQuery(Guid.NewGuid());

        _documentService.GenerateBatchPharmacyLabelsAsync(
            query.PrescriptionId, Arg.Any<CancellationToken>())
            .Returns<byte[]>(_ => throw new InvalidOperationException("Not found"));

        // Act
        var result = await PrintBatchLabelsHandler.Handle(
            query, _documentService, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task Handle_CallsDocumentServiceWithCorrectPrescriptionId()
    {
        // Arrange
        var prescriptionId = Guid.NewGuid();
        var query = new PrintBatchLabelsQuery(prescriptionId);

        _documentService.GenerateBatchPharmacyLabelsAsync(
            prescriptionId, Arg.Any<CancellationToken>())
            .Returns(new byte[] { 0x25, 0x50, 0x44, 0x46 });

        // Act
        await PrintBatchLabelsHandler.Handle(query, _documentService, CancellationToken.None);

        // Assert
        await _documentService.Received(1).GenerateBatchPharmacyLabelsAsync(
            prescriptionId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PrescriptionWithEmptyItems_ReturnsDocumentServiceResult()
    {
        // Arrange - prescription with zero items; document service may return
        // empty PDF or throw depending on implementation
        var prescriptionId = Guid.NewGuid();
        var query = new PrintBatchLabelsQuery(prescriptionId);

        // Document service returns an empty PDF (valid behavior for zero items)
        _documentService.GenerateBatchPharmacyLabelsAsync(
            prescriptionId, Arg.Any<CancellationToken>())
            .Returns(new byte[] { 0x25, 0x50, 0x44, 0x46 }); // minimal PDF bytes

        // Act
        var result = await PrintBatchLabelsHandler.Handle(
            query, _documentService, CancellationToken.None);

        // Assert - handler delegates to document service, returns whatever it produces
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_PrescriptionWithEmptyItems_WhenServiceThrows_ReturnsNotFound()
    {
        // Arrange - document service throws for prescription with no items
        var prescriptionId = Guid.NewGuid();
        var query = new PrintBatchLabelsQuery(prescriptionId);

        _documentService.GenerateBatchPharmacyLabelsAsync(
            prescriptionId, Arg.Any<CancellationToken>())
            .Returns<byte[]>(_ => throw new InvalidOperationException("Prescription has no items"));

        // Act
        var result = await PrintBatchLabelsHandler.Handle(
            query, _documentService, CancellationToken.None);

        // Assert - handler catches InvalidOperationException and returns NotFound
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
