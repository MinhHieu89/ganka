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

    private static byte[] CreateJpegBytes(int totalSize = 1024)
    {
        var bytes = new byte[totalSize];
        bytes[0] = 0xFF; bytes[1] = 0xD8; bytes[2] = 0xFF;
        return bytes;
    }

    private static byte[] CreatePngBytes(int totalSize = 1024)
    {
        var bytes = new byte[totalSize];
        bytes[0] = 0x89; bytes[1] = 0x50; bytes[2] = 0x4E; bytes[3] = 0x47;
        return bytes;
    }

    private static byte[] CreateWebpBytes(int totalSize = 1024)
    {
        var bytes = new byte[totalSize];
        // RIFF....WEBP
        bytes[0] = 0x52; bytes[1] = 0x49; bytes[2] = 0x46; bytes[3] = 0x46;
        bytes[8] = 0x57; bytes[9] = 0x45; bytes[10] = 0x42; bytes[11] = 0x50;
        return bytes;
    }

    [Fact]
    public async Task Handle_ValidJpegImage_UploadsAndReturnsUrl()
    {
        // Arrange
        var data = CreateJpegBytes();
        var stream = new MemoryStream(data);
        var command = new UploadClinicLogoCommand(stream, "image/jpeg", "logo.jpg", data.Length);
        _blobService.UploadAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>())
            .Returns("https://storage.blob.core.windows.net/clinic-logos/test.jpg");

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
        var data = CreatePngBytes(2048);
        var stream = new MemoryStream(data);
        var command = new UploadClinicLogoCommand(stream, "image/png", "logo.png", data.Length);
        _blobService.UploadAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>())
            .Returns("https://storage.blob.core.windows.net/clinic-logos/test.png");

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
        var data = CreateWebpBytes(512);
        var stream = new MemoryStream(data);
        var command = new UploadClinicLogoCommand(stream, "image/webp", "logo.webp", data.Length);
        _blobService.UploadAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>())
            .Returns("https://storage.blob.core.windows.net/clinic-logos/test.webp");

        // Act
        var result = await UploadClinicLogoHandler.Handle(
            command, _blobService, _settingsService, _branchContext, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_FileOver5MB_ReturnsValidationError()
    {
        // Arrange - 6MB file size via FileSize parameter
        var data = CreateJpegBytes(100);
        var stream = new MemoryStream(data);
        var command = new UploadClinicLogoCommand(stream, "image/jpeg", "big-logo.jpg", 6 * 1024 * 1024);

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
        var command = new UploadClinicLogoCommand(stream, "application/pdf", "doc.pdf", 1024);

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
        var command = new UploadClinicLogoCommand(stream, contentType, "file.ext", 1024);

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
        var data = CreateJpegBytes();
        var stream = new MemoryStream(data);
        var command = new UploadClinicLogoCommand(stream, "image/jpeg", "logo.jpg", data.Length);
        _blobService.UploadAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>())
            .Returns("https://storage.blob.core.windows.net/clinic-logos/test.jpg");

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

    [Fact]
    public async Task Handle_ValidUpload_PersistsLogoUrlToSettings()
    {
        // Arrange
        var expectedBlobUrl = "https://storage.blob.core.windows.net/clinic-logos/test.jpg";
        var data = CreateJpegBytes();
        var stream = new MemoryStream(data);
        var command = new UploadClinicLogoCommand(stream, "image/jpeg", "logo.jpg", data.Length);
        _blobService.UploadAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>())
            .Returns(expectedBlobUrl);

        // Act
        var result = await UploadClinicLogoHandler.Handle(
            command, _blobService, _settingsService, _branchContext, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedBlobUrl);
        await _settingsService.Received(1).UpdateLogoUrlAsync(expectedBlobUrl, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_JpegMagicBytesMismatch_ReturnsValidationError()
    {
        // Arrange - claims to be JPEG but has PNG magic bytes
        var data = CreatePngBytes();
        var stream = new MemoryStream(data);
        var command = new UploadClinicLogoCommand(stream, "image/jpeg", "logo.jpg", data.Length);

        // Act
        var result = await UploadClinicLogoHandler.Handle(
            command, _blobService, _settingsService, _branchContext, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("magic bytes");
    }

    [Fact]
    public async Task Handle_PngMagicBytesMismatch_ReturnsValidationError()
    {
        // Arrange - claims to be PNG but has JPEG magic bytes
        var data = CreateJpegBytes();
        var stream = new MemoryStream(data);
        var command = new UploadClinicLogoCommand(stream, "image/png", "logo.png", data.Length);

        // Act
        var result = await UploadClinicLogoHandler.Handle(
            command, _blobService, _settingsService, _branchContext, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UsesFileSizeFromCommand_NotStreamLength()
    {
        // Arrange - FileSize is set explicitly larger than stream
        var data = CreateJpegBytes(100);
        var stream = new MemoryStream(data);
        var command = new UploadClinicLogoCommand(stream, "image/jpeg", "logo.jpg", 2048);
        _blobService.UploadAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>())
            .Returns("https://storage.blob.core.windows.net/clinic-logos/test.jpg");

        // Act
        var result = await UploadClinicLogoHandler.Handle(
            command, _blobService, _settingsService, _branchContext, CancellationToken.None);

        // Assert - should use FileSize (2048), not stream.Length (100)
        result.IsSuccess.Should().BeTrue();
    }
}
