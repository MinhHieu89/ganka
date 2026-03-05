using Clinical.Application.Features;
using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Clinical.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using Shared.Application;
using Shared.Application.Services;
using Shared.Domain;

namespace Clinical.Unit.Tests.Features;

public class UploadMedicalImageHandlerTests
{
    private readonly IVisitRepository _visitRepository = Substitute.For<IVisitRepository>();
    private readonly IMedicalImageRepository _imageRepository = Substitute.For<IMedicalImageRepository>();
    private readonly IAzureBlobService _blobService = Substitute.For<IAzureBlobService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IValidator<UploadMedicalImageCommand> _validator = Substitute.For<IValidator<UploadMedicalImageCommand>>();

    private static readonly BranchId DefaultBranchId = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    private void SetupValidValidator()
    {
        _validator.ValidateAsync(Arg.Any<UploadMedicalImageCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private static Visit CreateVisit()
    {
        return Visit.Create(
            Guid.NewGuid(), "Patient A", Guid.NewGuid(), "Dr. A",
            DefaultBranchId, false);
    }

    [Fact]
    public async Task Handle_ValidJpegImage_UploadsAndCreatesRecord()
    {
        // Arrange
        SetupValidValidator();
        var visit = CreateVisit();
        var userId = Guid.NewGuid();
        _currentUser.UserId.Returns(userId);
        using var stream = new MemoryStream(new byte[5 * 1024 * 1024]); // 5MB
        var command = new UploadMedicalImageCommand(
            visit.Id, stream, "test-image.jpg", "image/jpeg", stream.Length,
            (int)ImageType.Fluorescein, (int)EyeTag.OD);

        _visitRepository.GetByIdAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);
        _blobService.UploadAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>())
            .Returns("https://blob.storage/clinical-images/test");

        // Act
        var result = await UploadMedicalImageHandler.Handle(
            command, _visitRepository, _imageRepository, _blobService,
            _unitOfWork, _currentUser, _validator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _blobService.Received(1).UploadAsync(
            "clinical-images", Arg.Any<string>(), stream, "image/jpeg");
        await _imageRepository.Received(1).AddAsync(
            Arg.Is<MedicalImage>(img =>
                img.VisitId == visit.Id &&
                img.UploadedById == userId &&
                img.Type == ImageType.Fluorescein &&
                img.ContentType == "image/jpeg"),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidVideoFile_UploadsWithExtendedSizeLimit()
    {
        // Arrange
        SetupValidValidator();
        var visit = CreateVisit();
        _currentUser.UserId.Returns(Guid.NewGuid());
        using var stream = new MemoryStream(new byte[1024]); // small stream, size in command
        var command = new UploadMedicalImageCommand(
            visit.Id, stream, "lacrimal-duct.mp4", "video/mp4", 150L * 1024 * 1024, // 150MB
            (int)ImageType.Video, null);

        _visitRepository.GetByIdAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);
        _blobService.UploadAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>())
            .Returns("https://blob.storage/clinical-images/video");

        // Act
        var result = await UploadMedicalImageHandler.Handle(
            command, _visitRepository, _imageRepository, _blobService,
            _unitOfWork, _currentUser, _validator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ImageTooLarge_ReturnsValidationError()
    {
        // Arrange - image > 50MB
        var command = new UploadMedicalImageCommand(
            Guid.NewGuid(), Stream.Null, "large.jpg", "image/jpeg", 55L * 1024 * 1024,
            (int)ImageType.Fluorescein, null);

        _validator.ValidateAsync(Arg.Any<UploadMedicalImageCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("FileSize", "Image file size must not exceed 50 MB.")
            }));

        // Act
        var result = await UploadMedicalImageHandler.Handle(
            command, _visitRepository, _imageRepository, _blobService,
            _unitOfWork, _currentUser, _validator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Handle_VideoTooLarge_ReturnsValidationError()
    {
        // Arrange - video > 200MB
        var command = new UploadMedicalImageCommand(
            Guid.NewGuid(), Stream.Null, "large.mp4", "video/mp4", 210L * 1024 * 1024,
            (int)ImageType.Video, null);

        _validator.ValidateAsync(Arg.Any<UploadMedicalImageCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("FileSize", "Video file size must not exceed 200 MB.")
            }));

        // Act
        var result = await UploadMedicalImageHandler.Handle(
            command, _visitRepository, _imageRepository, _blobService,
            _unitOfWork, _currentUser, _validator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Handle_InvalidContentType_ReturnsValidationError()
    {
        // Arrange - exe file
        var command = new UploadMedicalImageCommand(
            Guid.NewGuid(), Stream.Null, "malware.exe", "application/x-msdownload", 1024,
            (int)ImageType.Fluorescein, null);

        _validator.ValidateAsync(Arg.Any<UploadMedicalImageCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[]
            {
                new ValidationFailure("ContentType", "File type 'application/x-msdownload' is not allowed.")
            }));

        // Act
        var result = await UploadMedicalImageHandler.Handle(
            command, _visitRepository, _imageRepository, _blobService,
            _unitOfWork, _currentUser, _validator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Handle_VisitNotFound_ReturnsFailure()
    {
        // Arrange
        SetupValidValidator();
        _currentUser.UserId.Returns(Guid.NewGuid());
        var command = new UploadMedicalImageCommand(
            Guid.NewGuid(), Stream.Null, "test.jpg", "image/jpeg", 1024,
            (int)ImageType.Fluorescein, null);

        _visitRepository.GetByIdAsync(command.VisitId, Arg.Any<CancellationToken>()).Returns((Visit?)null);

        // Act
        var result = await UploadMedicalImageHandler.Handle(
            command, _visitRepository, _imageRepository, _blobService,
            _unitOfWork, _currentUser, _validator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task Handle_SignedVisit_StillSucceeds_AppendOnly()
    {
        // Arrange - images are append-only, should succeed even after sign-off
        SetupValidValidator();
        var visit = CreateVisit();
        visit.SignOff(Guid.NewGuid()); // Signed off
        _currentUser.UserId.Returns(Guid.NewGuid());
        using var stream = new MemoryStream(new byte[1024]);
        var command = new UploadMedicalImageCommand(
            visit.Id, stream, "post-signoff.jpg", "image/jpeg", 1024,
            (int)ImageType.OCT, (int)EyeTag.OS);

        _visitRepository.GetByIdAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);
        _blobService.UploadAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>())
            .Returns("https://blob.storage/clinical-images/test");

        // Act
        var result = await UploadMedicalImageHandler.Handle(
            command, _visitRepository, _imageRepository, _blobService,
            _unitOfWork, _currentUser, _validator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
