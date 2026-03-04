using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shared.Domain;
using Shared.Infrastructure;

namespace Clinical.Unit.Tests.Features;

public class SearchIcd10CodesHandlerTests
{
    private readonly IDoctorIcd10FavoriteRepository _favoriteRepository = Substitute.For<IDoctorIcd10FavoriteRepository>();

    private static ReferenceDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ReferenceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ReferenceDbContext(options);

        // Seed test data
        context.Icd10Codes.AddRange(
            Icd10Code.Create("H04.121", "Dry eye syndrome, right eye", "Hoi chung kho mat, mat phai", "Dry Eye", true),
            Icd10Code.Create("H04.122", "Dry eye syndrome, left eye", "Hoi chung kho mat, mat trai", "Dry Eye", true),
            Icd10Code.Create("H40.11", "Primary open-angle glaucoma", "Glaucom goc mo nguyen phat", "Glaucoma", false),
            Icd10Code.Create("H52.1", "Myopia", "Can thi", "Refractive", false),
            Icd10Code.Create("H52.4", "Presbyopia", "Lao thi", "Refractive", false)
        );
        context.SaveChanges();
        return context;
    }

    [Fact]
    public async Task Handle_SearchByEnglishDescription_ReturnsMatchingCodes()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var query = new SearchIcd10CodesQuery("Dry eye", null);

        // Act
        var result = await SearchIcd10CodesHandler.Handle(
            query, context, _favoriteRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.DescriptionEn.Contains("Dry eye", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Handle_SearchByVietnameseDescription_ReturnsMatchingCodes()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var query = new SearchIcd10CodesQuery("kho mat", null);

        // Act
        var result = await SearchIcd10CodesHandler.Handle(
            query, context, _favoriteRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.DescriptionVi.Contains("kho mat", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Handle_SearchByCodePrefix_ReturnsMatchingCodes()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var query = new SearchIcd10CodesQuery("H52", null);

        // Act
        var result = await SearchIcd10CodesHandler.Handle(
            query, context, _favoriteRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.Code.StartsWith("H52"));
    }

    [Fact]
    public async Task Handle_WithDoctorFavorites_FavoritesPinnedToTop()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var doctorId = Guid.NewGuid();
        _favoriteRepository.GetByDoctorIdAsync(doctorId, Arg.Any<CancellationToken>())
            .Returns(new List<string> { "H52.4" }); // Presbyopia is a favorite

        var query = new SearchIcd10CodesQuery("H52", doctorId);

        // Act
        var result = await SearchIcd10CodesHandler.Handle(
            query, context, _favoriteRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].Code.Should().Be("H52.4"); // Favorite pinned to top
        result[0].IsFavorite.Should().BeTrue();
        result[1].IsFavorite.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_NoMatches_ReturnsEmptyList()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var query = new SearchIcd10CodesQuery("xyznonexistent", null);

        // Act
        var result = await SearchIcd10CodesHandler.Handle(
            query, context, _favoriteRepository, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_SearchByPartialCode_ReturnsMatchingCodes()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var query = new SearchIcd10CodesQuery("H04", null);

        // Act
        var result = await SearchIcd10CodesHandler.Handle(
            query, context, _favoriteRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }
}
