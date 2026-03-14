using FluentAssertions;
using NSubstitute;
using Pharmacy.Application.Features.OtcSales;
using Pharmacy.Application.Interfaces;
using Pharmacy.Domain.Entities;
using Pharmacy.Domain.Enums;
using Shared.Domain;

namespace Pharmacy.Unit.Tests.Features;

public class GetDrugAvailableStockHandlerTests
{
    private readonly IDrugBatchRepository _batchRepository = Substitute.For<IDrugBatchRepository>();

    [Fact]
    public async Task Handle_WithAvailableBatches_ReturnsSumOfAvailableQuantity()
    {
        // Arrange
        var drugId = Guid.NewGuid();
        var batches = new List<DrugBatch>
        {
            CreateBatch(drugId, 50, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30))),
            CreateBatch(drugId, 30, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(60))),
            CreateBatch(drugId, 20, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(90)))
        };

        _batchRepository.GetAvailableBatchesFEFOAsync(drugId, Arg.Any<CancellationToken>())
            .Returns(batches);

        var query = new GetDrugAvailableStockQuery(drugId);

        // Act
        var result = await GetDrugAvailableStockHandler.Handle(query, _batchRepository, CancellationToken.None);

        // Assert
        result.Should().Be(100);
    }

    [Fact]
    public async Task Handle_WithNoBatches_Returns0()
    {
        // Arrange
        var drugId = Guid.NewGuid();
        _batchRepository.GetAvailableBatchesFEFOAsync(drugId, Arg.Any<CancellationToken>())
            .Returns(new List<DrugBatch>());

        var query = new GetDrugAvailableStockQuery(drugId);

        // Act
        var result = await GetDrugAvailableStockHandler.Handle(query, _batchRepository, CancellationToken.None);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithSingleBatch_ReturnsThatBatchQuantity()
    {
        // Arrange
        var drugId = Guid.NewGuid();
        var batches = new List<DrugBatch>
        {
            CreateBatch(drugId, 42, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)))
        };

        _batchRepository.GetAvailableBatchesFEFOAsync(drugId, Arg.Any<CancellationToken>())
            .Returns(batches);

        var query = new GetDrugAvailableStockQuery(drugId);

        // Act
        var result = await GetDrugAvailableStockHandler.Handle(query, _batchRepository, CancellationToken.None);

        // Assert
        result.Should().Be(42);
    }

    private static DrugBatch CreateBatch(Guid drugCatalogItemId, int quantity, DateOnly expiryDate)
    {
        return DrugBatch.Create(
            drugCatalogItemId: drugCatalogItemId,
            supplierId: Guid.NewGuid(),
            batchNumber: $"BATCH-{Guid.NewGuid():N}"[..12],
            expiryDate: expiryDate,
            quantity: quantity,
            purchasePrice: 10000m);
    }
}
