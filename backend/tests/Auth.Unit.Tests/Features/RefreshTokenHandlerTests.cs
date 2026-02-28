using Auth.Application.Features;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Auth.Unit.Tests.Features;

public class RefreshTokenHandlerTests
{
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IJwtService _jwtService = Substitute.For<IJwtService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ILogger<RefreshTokenHandler> _logger = Substitute.For<ILogger<RefreshTokenHandler>>();

    private RefreshTokenHandler CreateSut() => new(
        _refreshTokenRepository, _userRepository, _jwtService, _unitOfWork, _logger);

    [Fact]
    public async Task Handle_ValidToken_ReturnsNewTokenPair()
    {
        // Arrange
        var permission = TestHelpers.CreatePermission(PermissionModule.Auth, PermissionAction.View);
        var role = TestHelpers.CreateRole("Doctor", "Doctor role", false, permission);
        var user = TestHelpers.CreateFullyWiredUser("doc@test.com", "Doctor", "hash", true, (role, [permission]));

        var existingToken = TestHelpers.CreateRefreshTokenWithUser(user, "old-token", DateTime.UtcNow.AddDays(7));
        var command = new RefreshTokenCommand("old-token");

        _refreshTokenRepository.GetByTokenAsync("old-token", Arg.Any<CancellationToken>())
            .Returns(existingToken);
        _jwtService.GenerateAccessToken(user, Arg.Any<IEnumerable<string>>())
            .Returns(("new-access-token", DateTime.UtcNow.AddHours(1)));
        _jwtService.GenerateRefreshToken().Returns("new-refresh-token");
        _jwtService.GetRefreshTokenLifetimeDays(false).Returns(7);

        var sut = CreateSut();

        // Act
        var result = await sut.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("new-access-token");
        result.Value.RefreshToken.Should().Be("new-refresh-token");
        result.Value.User.Email.Should().Be("doc@test.com");
    }

    [Fact]
    public async Task Handle_NonExistentToken_ReturnsFailure()
    {
        // Arrange
        _refreshTokenRepository.GetByTokenAsync("nonexistent", Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        var command = new RefreshTokenCommand("nonexistent");
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Unauthorized");
    }

    [Fact]
    public async Task Handle_ExpiredToken_ReturnsFailure()
    {
        // Arrange
        var user = TestHelpers.CreateUser();
        var expiredToken = TestHelpers.CreateRefreshTokenWithUser(
            user, "expired-token", DateTime.UtcNow.AddDays(-1));

        _refreshTokenRepository.GetByTokenAsync("expired-token", Arg.Any<CancellationToken>())
            .Returns(expiredToken);

        var command = new RefreshTokenCommand("expired-token");
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Unauthorized");
    }

    [Fact]
    public async Task Handle_RevokedToken_RevokesTokenFamily()
    {
        // Arrange
        var user = TestHelpers.CreateUser();
        var familyId = Guid.NewGuid();
        var revokedToken = TestHelpers.CreateRefreshTokenWithUser(
            user, "reused-token", DateTime.UtcNow.AddDays(7), familyId, isRevoked: true);

        _refreshTokenRepository.GetByTokenAsync("reused-token", Arg.Any<CancellationToken>())
            .Returns(revokedToken);

        var command = new RefreshTokenCommand("reused-token");
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Unauthorized");
        await _refreshTokenRepository.Received(1)
            .RevokeAllByFamilyIdAsync(familyId, "Token reuse detected", Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
