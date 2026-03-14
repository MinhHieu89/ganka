using FluentAssertions;
using NSubstitute;
using Shared.Application;
using Shared.Application.Features;
using Shared.Application.Interfaces;
using Shared.Application.Services;
using Shared.Domain;
using Shared.Infrastructure.Entities;

namespace Shared.Unit.Tests.Features;

public class UploadClinicLogoHandlerTests
{
    private readonly IAzureBlobService _blobService = Substitute.For<IAzureBlobService>();
    private readonly IClinicSettingsService _settingsService = Substitute.For<IClinicSettingsService>();
    private readonly IBranchContext _branchContext = Substitute.For<IBranchContext>();

    private static readonly Guid TestBranchId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public UploadClinicLogoHandlerTests()
    {
        _branchContext.CurrentBranchId.Returns(TestBranchId);
    }

    [Fact]
    public async Task Handle_ValidJpegImage_UploadsAndReturnsUrl()
    {
        // Arrange
        var stream = new MemoryStream(new byte[1024]);
        var command = new UploadClinicLogoCommand(stream, "image/jpeg", "logo.jpg");
        _blobService.UploadAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>())
            .Returns("https://storage.blob.core.windows.net/clinic-logos/test.jpg");
        _settingsService.GetCurrentAsync(Arg.Any<CancellationToken>())
            .Returns(new ClinicSettingsDto(Guid.NewGuid(), "Test Clinic", null, "123 Street", null, null, null, null, null, null, null));

        // Act
        var result = await UploadClinicLogoHandler.Handle(
            command, _blobService, _settingsService, _branchContext, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("clinic-logos");
        await _blobService.Received(1).UploadAsync(
            "clinic-logos", Arg.Any<string>(), Arg.Any<Stream>(), "image/jpeg");
    }

    [Fact]
    public async Task Handle_ValidPngImage_UploadsSuccessfully()
    {
        // Arrange
        var stream = new MemoryStream(new byte[2048]);
        var command = new UploadClinicLogoCommand(stream, "image/png", "logo.png");
        _blobService.UploadAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>())
            .Returns("https://storage.blob.core.windows.net/clinic-logos/test.png");
        _settingsService.GetCurrentAsync(Arg.Any<CancellationToken>())
            .Returns(new ClinicSettingsDto(Guid.NewGuid(), "Test Clinic", null, "123 Street", null, null, null, null, null, null, null));

        // Act
        var result = await UploadClinicLogoHandler.Handle(
            command, _blobService, _settingsService, _branchContext, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidWebpImage_UploadsSuccessfully()
    {
        // Arrange
        var stream = new MemoryStream(new byte[512]);
        var command = new UploadClinicLogoCommand(stream, "image/webp", "logo.webp");
        _blobService.UploadAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>())
            .Returns("https://storage.blob.core.windows.net/clinic-logos/test.webp");
        _settingsService.GetCurrentAsync(Arg.Any<CancellationToken>())
            .Returns(new ClinicSettingsDto(Guid.NewGuid(), "Test Clinic", null, "123 Street", null, null, null, null, null, null, null));

        // Act
        var result = await UploadClinicLogoHandler.Handle(
            command, _blobService, _settingsService, _branchContext, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_FileOver5MB_ReturnsValidationError()
    {
        // Arrange - 6MB file
        var stream = new MemoryStream(new byte[6 * 1024 * 1024]);
        var command = new UploadClinicLogoCommand(stream, "image/jpeg", "big-logo.jpg");

        // Act
        var result = await UploadClinicLogoHandler.Handle(
            command, _blobService, _settingsService, _branchContext, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
        await _blobService.DidNotReceive().UploadAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_NonImageContentType_ReturnsValidationError()
    {
        // Arrange
        var stream = new MemoryStream(new byte[1024]);
        var command = new UploadClinicLogoCommand(stream, "application/pdf", "doc.pdf");

        // Act
        var result = await UploadClinicLogoHandler.Handle(
            command, _blobService, _settingsService, _branchContext, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Theory]
    [InlineData("text/plain")]
    [InlineData("application/json")]
    [InlineData("video/mp4")]
    public async Task Handle_InvalidMimeTypes_ReturnsValidationError(string contentType)
    {
        // Arrange
        var stream = new MemoryStream(new byte[1024]);
        var command = new UploadClinicLogoCommand(stream, contentType, "file.ext");

        // Act
        var result = await UploadClinicLogoHandler.Handle(
            command, _blobService, _settingsService, _branchContext, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_BlobNameContainsBranchId()
    {
        // Arrange
        var stream = new MemoryStream(new byte[1024]);
        var command = new UploadClinicLogoCommand(stream, "image/jpeg", "logo.jpg");
        _blobService.UploadAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>())
            .Returns("https://storage.blob.core.windows.net/clinic-logos/test.jpg");
        _settingsService.GetCurrentAsync(Arg.Any<CancellationToken>())
            .Returns(new ClinicSettingsDto(Guid.NewGuid(), "Test Clinic", null, "123 Street", null, null, null, null, null, null, null));

        // Act
        await UploadClinicLogoHandler.Handle(
            command, _blobService, _settingsService, _branchContext, CancellationToken.None);

        // Assert
        await _blobService.Received(1).UploadAsync(
            "clinic-logos",
            Arg.Is<string>(s => s.Contains(TestBranchId.ToString())),
            Arg.Any<Stream>(),
            "image/jpeg");
    }
}
