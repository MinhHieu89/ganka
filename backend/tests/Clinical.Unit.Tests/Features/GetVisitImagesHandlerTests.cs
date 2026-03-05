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

public class GetVisitImagesHandlerTests
{
    private readonly IMedicalImageRepository _imageRepository = Substitute.For<IMedicalImageRepository>();
    private readonly IAzureBlobService _blobService = Substitute.For<IAzureBlobService>();

    [Fact]
    public async Task Handle_VisitWithImages_ReturnsDtosWithSasUrls()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var images = new List<MedicalImage>
        {
            CreateImage(visitId, ImageType.Fluorescein, "img1.jpg", "image/jpeg", 1024),
            CreateImage(visitId, ImageType.Meibography, "img2.png", "image/png", 2048),
            CreateImage(visitId, ImageType.OCT, "img3.tiff", "image/tiff", 4096),
            CreateImage(visitId, ImageType.SpecularMicroscopy, "img4.jpg", "image/jpeg", 512),
            CreateImage(visitId, ImageType.Topography, "img5.bmp", "image/bmp", 8192),
        };

        _imageRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>()).Returns(images);
        _blobService.GetSasUrlAsync("clinical-images", Arg.Any<string>(), Arg.Any<TimeSpan>())
            .Returns(callInfo => $"https://blob.storage/sas/{callInfo.ArgAt<string>(1)}");

        var query = new GetVisitImagesQuery(visitId);

        // Act
        var result = await GetVisitImagesHandler.Handle(query, _imageRepository, _blobService, CancellationToken.None);

        // Assert
        result.Should().HaveCount(5);
        result.Should().AllSatisfy(dto => dto.Url.Should().StartWith("https://blob.storage/sas/"));
        await _blobService.Received(5).GetSasUrlAsync(
            "clinical-images", Arg.Any<string>(), TimeSpan.FromHours(1));
    }

    [Fact]
    public async Task Handle_VisitWithNoImages_ReturnsEmptyList()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        _imageRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>())
            .Returns(new List<MedicalImage>());

        var query = new GetVisitImagesQuery(visitId);

        // Act
        var result = await GetVisitImagesHandler.Handle(query, _imageRepository, _blobService, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_SasUrls_Have1HourExpiry()
    {
        // Arrange
        var visitId = Guid.NewGuid();
        var images = new List<MedicalImage>
        {
            CreateImage(visitId, ImageType.Fluorescein, "img1.jpg", "image/jpeg", 1024),
        };

        _imageRepository.GetByVisitIdAsync(visitId, Arg.Any<CancellationToken>()).Returns(images);
        _blobService.GetSasUrlAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<TimeSpan>())
            .Returns("https://blob.storage/sas/test");

        var query = new GetVisitImagesQuery(visitId);

        // Act
        await GetVisitImagesHandler.Handle(query, _imageRepository, _blobService, CancellationToken.None);

        // Assert
        await _blobService.Received(1).GetSasUrlAsync(
            "clinical-images", Arg.Any<string>(), TimeSpan.FromHours(1));
    }

    private static MedicalImage CreateImage(
        Guid visitId, ImageType type, string fileName, string contentType, long size)
    {
        return MedicalImage.Create(
            visitId, Guid.NewGuid(), type, EyeTag.OD,
            fileName, $"clinical-images/{visitId}/{fileName}",
            contentType, size);
    }
}
