using Clinical.Application.Features.Prescriptions;
using Clinical.Application.Interfaces;
using Clinical.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Shared.Domain;

namespace Clinical.Unit.Tests.Features;

public class PrintBatchLabelsHandlerTests
{
    private readonly IVisitRepository _visitRepository = Substitute.For<IVisitRepository>();
    private readonly IDocumentService _documentService = Substitute.For<IDocumentService>();

    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    private static Visit CreateVisitWithPrescription(out DrugPrescription prescription, int itemCount = 2)
    {
        var visit = Visit.Create(
            Guid.NewGuid(), "Patient A", Guid.NewGuid(), "Dr. A",
            DefaultBranchId, false);

        prescription = DrugPrescription.Create(visit.Id, "Test notes");

        for (var i = 0; i < itemCount; i++)
        {
            var item = PrescriptionItem.CreateFromCatalog(
                prescription.Id,
                Guid.NewGuid(),
                $"Drug {i + 1}",
                $"Generic {i + 1}",
                "0.5%",
                0, // Drops
                0, // Topical
                "1 drop, 3 times/day",
                null,
                1,
                "bottle",
                "3 times/day",
                7,
                false,
                i);
            prescription.AddItem(item);
        }

        visit.AddDrugPrescription(prescription);
        return visit;
    }

    [Fact]
    public async Task Handle_ValidPrescription_ReturnsPdfBytes()
    {
        // Arrange
        var visit = CreateVisitWithPrescription(out var prescription);
        var query = new PrintBatchLabelsQuery(prescription.Id);

        _visitRepository.GetByIdWithDetailsAsync(visit.Id, Arg.Any<CancellationToken>())
            .Returns(visit);
        _documentService.GenerateBatchPharmacyLabelsAsync(
            prescription.Id, Arg.Any<CancellationToken>())
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
}
