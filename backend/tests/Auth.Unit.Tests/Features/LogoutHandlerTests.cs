using Auth.Application.Features;
using Auth.Application.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace Auth.Unit.Tests.Features;

public class LogoutHandlerTests
{
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private LogoutHandler CreateSut() => new(_refreshTokenRepository, _unitOfWork);

    [Fact]
    public async Task Handle_ValidUserId_RevokesAllTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new LogoutCommand(userId);
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _refreshTokenRepository.Received(1)
            .RevokeAllByUserIdAsync(userId, "User logout", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CallsSaveChanges()
    {
        // Arrange
        var command = new LogoutCommand(Guid.NewGuid());
        var sut = CreateSut();

        // Act
        await sut.Handle(command);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
