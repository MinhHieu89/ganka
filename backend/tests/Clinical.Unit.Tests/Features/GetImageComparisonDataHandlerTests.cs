using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Shared.Application.Services;
using Shared.Domain;

namespace Clinical.Unit.Tests.Features;

public class GetImageComparisonDataHandlerTests
{
    private readonly IVisitRepository _visitRepository = Substitute.For<IVisitRepository>();
    private readonly IMedicalImageRepository _imageRepository = Substitute.For<IMedicalImageRepository>();
    private readonly IAzureBlobService _blobService = Substitute.For<IAzureBlobService>();

    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    [Fact]
    public async Task Handle_TwoVisitsSamePatientSameType_ReturnsBothVisitsImages()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var visit1 = Visit.Create(patientId, "Patient A", Guid.NewGuid(), "Dr. A", DefaultBranchId, false);
        var visit2 = Visit.Create(patientId, "Patient A", Guid.NewGuid(), "Dr. A", DefaultBranchId, false);

        var images1 = new List<MedicalImage>
        {
            MedicalImage.Create(visit1.Id, Guid.NewGuid(), ImageType.Meibography, EyeTag.OD, "meibo1.jpg", "blob1", "image/jpeg", 1024),
            MedicalImage.Create(visit1.Id, Guid.NewGuid(), ImageType.Meibography, EyeTag.OS, "meibo2.jpg", "blob2", "image/jpeg", 2048),
        };

        var images2 = new List<MedicalImage>
        {
            MedicalImage.Create(visit2.Id, Guid.NewGuid(), ImageType.Meibography, EyeTag.OD, "meibo3.jpg", "blob3", "image/jpeg", 1024),
        };

        _visitRepository.GetByIdAsync(visit1.Id, Arg.Any<CancellationToken>()).Returns(visit1);
        _visitRepository.GetByIdAsync(visit2.Id, Arg.Any<CancellationToken>()).Returns(visit2);
        _imageRepository.GetByVisitIdAndTypeAsync(visit1.Id, ImageType.Meibography, Arg.Any<CancellationToken>()).Returns(images1);
        _imageRepository.GetByVisitIdAndTypeAsync(visit2.Id, ImageType.Meibography, Arg.Any<CancellationToken>()).Returns(images2);
        _blobService.GetSasUrlAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan>())
            .Returns("https://blob.storage/sas/test");

        var query = new GetImageComparisonQuery(patientId, visit1.Id, visit2.Id, (int)ImageType.Meibography);

        // Act
        var result = await GetImageComparisonDataHandler.Handle(
            query, _visitRepository, _imageRepository, _blobService, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Visit1Images.Should().HaveCount(2);
        result.Value.Visit2Images.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_VisitsBelongToDifferentPatients_ReturnsFailure()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var otherPatientId = Guid.NewGuid();
        var visit1 = Visit.Create(patientId, "Patient A", Guid.NewGuid(), "Dr. A", DefaultBranchId, false);
        var visit2 = Visit.Create(otherPatientId, "Patient B", Guid.NewGuid(), "Dr. B", DefaultBranchId, false);

        _visitRepository.GetByIdAsync(visit1.Id, Arg.Any<CancellationToken>()).Returns(visit1);
        _visitRepository.GetByIdAsync(visit2.Id, Arg.Any<CancellationToken>()).Returns(visit2);

        var query = new GetImageComparisonQuery(patientId, visit1.Id, visit2.Id, (int)ImageType.Meibography);

        // Act
        var result = await GetImageComparisonDataHandler.Handle(
            query, _visitRepository, _imageRepository, _blobService, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Handle_OneVisitHasNoImages_ReturnsEmptyListForThatVisit()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var visit1 = Visit.Create(patientId, "Patient A", Guid.NewGuid(), "Dr. A", DefaultBranchId, false);
        var visit2 = Visit.Create(patientId, "Patient A", Guid.NewGuid(), "Dr. A", DefaultBranchId, false);

        var images1 = new List<MedicalImage>
        {
            MedicalImage.Create(visit1.Id, Guid.NewGuid(), ImageType.OCT, EyeTag.OD, "oct1.jpg", "blob1", "image/jpeg", 1024),
        };

        _visitRepository.GetByIdAsync(visit1.Id, Arg.Any<CancellationToken>()).Returns(visit1);
        _visitRepository.GetByIdAsync(visit2.Id, Arg.Any<CancellationToken>()).Returns(visit2);
        _imageRepository.GetByVisitIdAndTypeAsync(visit1.Id, ImageType.OCT, Arg.Any<CancellationToken>()).Returns(images1);
        _imageRepository.GetByVisitIdAndTypeAsync(visit2.Id, ImageType.OCT, Arg.Any<CancellationToken>())
            .Returns(new List<MedicalImage>());
        _blobService.GetSasUrlAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan>())
            .Returns("https://blob.storage/sas/test");

        var query = new GetImageComparisonQuery(patientId, visit1.Id, visit2.Id, (int)ImageType.OCT);

        // Act
        var result = await GetImageComparisonDataHandler.Handle(
            query, _visitRepository, _imageRepository, _blobService, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Visit1Images.Should().HaveCount(1);
        result.Value.Visit2Images.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_VisitNotFound_ReturnsFailure()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        _visitRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Visit?)null);

        var query = new GetImageComparisonQuery(patientId, Guid.NewGuid(), Guid.NewGuid(), (int)ImageType.Fluorescein);

        // Act
        var result = await GetImageComparisonDataHandler.Handle(
            query, _visitRepository, _imageRepository, _blobService, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
