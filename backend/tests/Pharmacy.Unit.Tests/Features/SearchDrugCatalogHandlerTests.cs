using FluentAssertions;
using NSubstitute;
using Pharmacy.Application.Features;
using Pharmacy.Application.Interfaces;
using Pharmacy.Contracts.Dtos;

namespace Pharmacy.Unit.Tests.Features;

public class SearchDrugCatalogHandlerTests
{
    private readonly IDrugCatalogItemRepository _repository = Substitute.For<IDrugCatalogItemRepository>();

    [Fact]
    public async Task Handle_WithMatchingTerm_ReturnsResults()
    {
        // Arrange
        var expectedItems = new List<DrugCatalogItemDto>
        {
            new(Guid.NewGuid(), "Tobramycin 0.3%", "Tobramycin 0,3%", "Tobramycin",
                0, "0.3%", 0, "Chai", "1-2 drops x 4 times/day", true),
            new(Guid.NewGuid(), "Tobramycin/Dexamethasone", "Tobramycin/Dexamethasone", "Tobramycin",
                0, "0.3%/0.1%", 0, "Chai", "1-2 drops x 3 times/day", true)
        };

        _repository.SearchAsync("Tobramycin", Arg.Any<CancellationToken>())
            .Returns(expectedItems);

        var query = new SearchDrugCatalogQuery("Tobramycin");

        // Act
        var result = await SearchDrugCatalogHandler.Handle(query, _repository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(item => item.Name.Contains("Tobramycin"));
    }

    [Fact]
    public async Task Handle_WithEmptyTerm_ReturnsEmpty()
    {
        // Arrange
        var query = new SearchDrugCatalogQuery("");

        // Act
        var result = await SearchDrugCatalogHandler.Handle(query, _repository, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        await _repository.DidNotReceive().SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithWhitespaceTerm_ReturnsEmpty()
    {
        // Arrange
        var query = new SearchDrugCatalogQuery("   ");

        // Act
        var result = await SearchDrugCatalogHandler.Handle(query, _repository, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        await _repository.DidNotReceive().SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TrimsTerm_BeforeSearch()
    {
        // Arrange
        var expectedItems = new List<DrugCatalogItemDto>
        {
            new(Guid.NewGuid(), "Tobramycin 0.3%", "Tobramycin 0,3%", "Tobramycin",
                0, "0.3%", 0, "Chai", "1-2 drops x 4 times/day", true)
        };

        _repository.SearchAsync("Tobramycin", Arg.Any<CancellationToken>())
            .Returns(expectedItems);

        var query = new SearchDrugCatalogQuery("  Tobramycin  ");

        // Act
        var result = await SearchDrugCatalogHandler.Handle(query, _repository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        await _repository.Received(1).SearchAsync("Tobramycin", Arg.Any<CancellationToken>());
    }
}
