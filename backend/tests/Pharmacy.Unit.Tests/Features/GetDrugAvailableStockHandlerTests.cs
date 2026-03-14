using FluentAssertions;
using NSubstitute;
using Pharmacy.Application.Features.OtcSales;
using Pharmacy.Application.Interfaces;

namespace Pharmacy.Unit.Tests.Features;

public class GetDrugAvailableStockHandlerTests
{
    private readonly IDrugBatchRepository _batchRepository = Substitute.For<IDrugBatchRepository>();

    [Fact]
    public async Task Handle_WithAvailableBatches_ReturnsTotalStock()
    {
        // Arrange
        var drugId = Guid.NewGuid();
        _batchRepository.GetTotalStockAsync(drugId, Arg.Any<CancellationToken>())
            .Returns(100);

        var query = new GetDrugAvailableStockQuery(drugId);

        // Act
        var result = await GetDrugAvailableStockHandler.Handle(query, _batchRepository, CancellationToken.None);

        // Assert
        result.Should().Be(100);
        await _batchRepository.Received(1).GetTotalStockAsync(drugId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNoBatches_Returns0()
    {
        // Arrange
        var drugId = Guid.NewGuid();
        _batchRepository.GetTotalStockAsync(drugId, Arg.Any<CancellationToken>())
            .Returns(0);

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
        _batchRepository.GetTotalStockAsync(drugId, Arg.Any<CancellationToken>())
            .Returns(42);

        var query = new GetDrugAvailableStockQuery(drugId);

        // Act
        var result = await GetDrugAvailableStockHandler.Handle(query, _batchRepository, CancellationToken.None);

        // Assert
        result.Should().Be(42);
    }
}
