using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using FluentAssertions;
using NSubstitute;
using Shared.Application.Interfaces;
using Shared.Domain;

namespace Clinical.Unit.Tests.Features;

public class SearchIcd10CodesHandlerTests
{
    private readonly IDoctorIcd10FavoriteRepository _favoriteRepository = Substitute.For<IDoctorIcd10FavoriteRepository>();
    private readonly IReferenceDataRepository _referenceDataRepository = Substitute.For<IReferenceDataRepository>();

    [Fact]
    public async Task Handle_SearchByEnglishDescription_ReturnsMatchingCodes()
    {
        // Arrange
        _referenceDataRepository.SearchAsync("Dry eye", 50, Arg.Any<CancellationToken>())
            .Returns(new List<Icd10Code>
            {
                Icd10Code.Create("H04.121", "Dry eye syndrome, right eye", "Hoi chung kho mat, mat phai", "Dry Eye", true),
                Icd10Code.Create("H04.122", "Dry eye syndrome, left eye", "Hoi chung kho mat, mat trai", "Dry Eye", true)
            });

        var query = new SearchIcd10CodesQuery("Dry eye", null);

        // Act
        var result = await SearchIcd10CodesHandler.Handle(
            query, _referenceDataRepository, _favoriteRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.DescriptionEn.Contains("Dry eye", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Handle_SearchByVietnameseDescription_ReturnsMatchingCodes()
    {
        // Arrange
        _referenceDataRepository.SearchAsync("kho mat", 50, Arg.Any<CancellationToken>())
            .Returns(new List<Icd10Code>
            {
                Icd10Code.Create("H04.121", "Dry eye syndrome, right eye", "Hoi chung kho mat, mat phai", "Dry Eye", true),
                Icd10Code.Create("H04.122", "Dry eye syndrome, left eye", "Hoi chung kho mat, mat trai", "Dry Eye", true)
            });

        var query = new SearchIcd10CodesQuery("kho mat", null);

        // Act
        var result = await SearchIcd10CodesHandler.Handle(
            query, _referenceDataRepository, _favoriteRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.DescriptionVi.Contains("kho mat", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Handle_SearchByCodePrefix_ReturnsMatchingCodes()
    {
        // Arrange
        _referenceDataRepository.SearchAsync("H52", 50, Arg.Any<CancellationToken>())
            .Returns(new List<Icd10Code>
            {
                Icd10Code.Create("H52.1", "Myopia", "Can thi", "Refractive", false),
                Icd10Code.Create("H52.4", "Presbyopia", "Lao thi", "Refractive", false)
            });

        var query = new SearchIcd10CodesQuery("H52", null);

        // Act
        var result = await SearchIcd10CodesHandler.Handle(
            query, _referenceDataRepository, _favoriteRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.Code.StartsWith("H52"));
    }

    [Fact]
    public async Task Handle_WithDoctorFavorites_FavoritesPinnedToTop()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        _referenceDataRepository.SearchAsync("H52", 50, Arg.Any<CancellationToken>())
            .Returns(new List<Icd10Code>
            {
                Icd10Code.Create("H52.1", "Myopia", "Can thi", "Refractive", false),
                Icd10Code.Create("H52.4", "Presbyopia", "Lao thi", "Refractive", false)
            });

        _favoriteRepository.GetByDoctorIdAsync(doctorId, Arg.Any<CancellationToken>())
            .Returns(new List<string> { "H52.4" }); // Presbyopia is a favorite

        var query = new SearchIcd10CodesQuery("H52", doctorId);

        // Act
        var result = await SearchIcd10CodesHandler.Handle(
            query, _referenceDataRepository, _favoriteRepository, CancellationToken.None);

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
        _referenceDataRepository.SearchAsync("xyznonexistent", 50, Arg.Any<CancellationToken>())
            .Returns(new List<Icd10Code>());

        var query = new SearchIcd10CodesQuery("xyznonexistent", null);

        // Act
        var result = await SearchIcd10CodesHandler.Handle(
            query, _referenceDataRepository, _favoriteRepository, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_SearchByPartialCode_ReturnsMatchingCodes()
    {
        // Arrange
        _referenceDataRepository.SearchAsync("H04", 50, Arg.Any<CancellationToken>())
            .Returns(new List<Icd10Code>
            {
                Icd10Code.Create("H04.121", "Dry eye syndrome, right eye", "Hoi chung kho mat, mat phai", "Dry Eye", true),
                Icd10Code.Create("H04.122", "Dry eye syndrome, left eye", "Hoi chung kho mat, mat trai", "Dry Eye", true)
            });

        var query = new SearchIcd10CodesQuery("H04", null);

        // Act
        var result = await SearchIcd10CodesHandler.Handle(
            query, _referenceDataRepository, _favoriteRepository, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }
}
