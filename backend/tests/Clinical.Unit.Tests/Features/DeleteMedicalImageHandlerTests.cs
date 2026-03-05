using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Shared.Application;
using Shared.Application.Services;
using Shared.Domain;

namespace Clinical.Unit.Tests.Features;

public class DeleteMedicalImageHandlerTests
{
    private readonly IMedicalImageRepository _imageRepository = Substitute.For<IMedicalImageRepository>();
    private readonly IAzureBlobService _blobService = Substitute.For<IAzureBlobService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    [Fact]
    public async Task Handle_ValidImageId_DeletesBlobAndDbRecord()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUser.UserId.Returns(userId);
        var image = MedicalImage.Create(
            Guid.NewGuid(), userId, ImageType.Fluorescein, EyeTag.OD,
            "test.jpg", "clinical-images/visit1/test.jpg", "image/jpeg", 1024);

        _imageRepository.GetByIdAsync(image.Id, Arg.Any<CancellationToken>()).Returns(image);
        _blobService.DeleteAsync("clinical-images", image.BlobName).Returns(true);

        var command = new DeleteMedicalImageCommand(image.Id);

        // Act
        var result = await DeleteMedicalImageHandler.Handle(
            command, _imageRepository, _blobService, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _blobService.Received(1).DeleteAsync("clinical-images", image.BlobName);
        _imageRepository.Received(1).Delete(image);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ImageNotFound_ReturnsFailure()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        _currentUser.UserId.Returns(Guid.NewGuid());
        _imageRepository.GetByIdAsync(imageId, Arg.Any<CancellationToken>()).Returns((MedicalImage?)null);

        var command = new DeleteMedicalImageCommand(imageId);

        // Act
        var result = await DeleteMedicalImageHandler.Handle(
            command, _imageRepository, _blobService, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task Handle_UploaderCanDelete_Succeeds()
    {
        // Arrange - user is the uploader
        var uploaderId = Guid.NewGuid();
        _currentUser.UserId.Returns(uploaderId);
        var image = MedicalImage.Create(
            Guid.NewGuid(), uploaderId, ImageType.Meibography, EyeTag.OS,
            "meibo.png", "clinical-images/visit1/meibo.png", "image/png", 2048);

        _imageRepository.GetByIdAsync(image.Id, Arg.Any<CancellationToken>()).Returns(image);
        _blobService.DeleteAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        var command = new DeleteMedicalImageCommand(image.Id);

        // Act
        var result = await DeleteMedicalImageHandler.Handle(
            command, _imageRepository, _blobService, _unitOfWork, _currentUser, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
