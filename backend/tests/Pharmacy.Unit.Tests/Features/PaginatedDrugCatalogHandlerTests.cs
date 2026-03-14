using FluentAssertions;
using NSubstitute;
using Pharmacy.Application.Features;
using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;

namespace Pharmacy.Unit.Tests.Features;

public class PaginatedDrugCatalogHandlerTests
{
    private readonly IDrugCatalogItemRepository _repository = Substitute.For<IDrugCatalogItemRepository>();

    private static List<DrugCatalogItemDto> CreateDrugList(int count)
    {
        return Enumerable.Range(1, count).Select(i =>
            new DrugCatalogItemDto(
                Guid.NewGuid(), $"Drug {i}", $"Thuoc {i}", $"Generic {i}",
                0, "10mg", 0, "Chai", null, true, 50000m, 10)
        ).ToList();
    }

    [Fact]
    public async Task Handle_WithPage1PageSize10_ReturnsFirst10ItemsAndTotalCount()
    {
        // Arrange
        var allItems = CreateDrugList(10);
        _repository.GetPaginatedAsync(1, 10, null, Arg.Any<CancellationToken>())
            .Returns((allItems, 25));

        var query = new PaginatedDrugCatalogQuery(Page: 1, PageSize: 10);

        // Act
        var result = await PaginatedDrugCatalogHandler.Handle(query, _repository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(10);
        result.Value.TotalCount.Should().Be(25);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
        result.Value.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_FiltersResults()
    {
        // Arrange
        var filtered = CreateDrugList(2);
        _repository.GetPaginatedAsync(1, 20, "amox", Arg.Any<CancellationToken>())
            .Returns((filtered, 2));

        var query = new PaginatedDrugCatalogQuery(Page: 1, PageSize: 20, Search: "amox");

        // Act
        var result = await PaginatedDrugCatalogHandler.Handle(query, _repository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithPage2_ReturnsSecondPage()
    {
        // Arrange
        var secondPage = CreateDrugList(5);
        _repository.GetPaginatedAsync(2, 10, null, Arg.Any<CancellationToken>())
            .Returns((secondPage, 15));

        var query = new PaginatedDrugCatalogQuery(Page: 2, PageSize: 10);

        // Act
        var result = await PaginatedDrugCatalogHandler.Handle(query, _repository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(5);
        result.Value.Page.Should().Be(2);
        result.Value.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ClampsPageSizeTo100()
    {
        // Arrange
        var items = CreateDrugList(20);
        _repository.GetPaginatedAsync(1, 100, null, Arg.Any<CancellationToken>())
            .Returns((items, 20));

        var query = new PaginatedDrugCatalogQuery(Page: 1, PageSize: 200);

        // Act
        var result = await PaginatedDrugCatalogHandler.Handle(query, _repository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PageSize.Should().Be(100);
        await _repository.Received(1).GetPaginatedAsync(1, 100, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithPageLessThan1_DefaultsTo1()
    {
        // Arrange
        var items = CreateDrugList(5);
        _repository.GetPaginatedAsync(1, 20, null, Arg.Any<CancellationToken>())
            .Returns((items, 5));

        var query = new PaginatedDrugCatalogQuery(Page: 0, PageSize: 20);

        // Act
        var result = await PaginatedDrugCatalogHandler.Handle(query, _repository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithNoResults_ReturnsTotalPages0()
    {
        // Arrange
        _repository.GetPaginatedAsync(1, 20, "nonexistent", Arg.Any<CancellationToken>())
            .Returns((new List<DrugCatalogItemDto>(), 0));

        var query = new PaginatedDrugCatalogQuery(Page: 1, PageSize: 20, Search: "nonexistent");

        // Act
        var result = await PaginatedDrugCatalogHandler.Handle(query, _repository, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.TotalPages.Should().Be(0);
    }
}
