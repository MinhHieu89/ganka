using Auth.Application.Features;
using Auth.Application.Interfaces;
using Auth.Contracts.Queries;
using Auth.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace Auth.Unit.Tests.Features;

public class VerifyManagerPinHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();

    [Fact]
    public async Task HandleAsync_EmptyPin_ReturnsFalse()
    {
        var query = new VerifyManagerPinQuery(Guid.NewGuid(), "");

        var result = await VerifyManagerPinHandler.HandleAsync(
            query, _userRepository, _passwordHasher, CancellationToken.None);

        result.IsValid.Should().BeFalse();
        await _userRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhitespacePin_ReturnsFalse()
    {
        var query = new VerifyManagerPinQuery(Guid.NewGuid(), "   ");

        var result = await VerifyManagerPinHandler.HandleAsync(
            query, _userRepository, _passwordHasher, CancellationToken.None);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ReturnsFalse()
    {
        var managerId = Guid.NewGuid();
        var query = new VerifyManagerPinQuery(managerId, "123456");
        _userRepository.GetByIdAsync(managerId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await VerifyManagerPinHandler.HandleAsync(
            query, _userRepository, _passwordHasher, CancellationToken.None);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_UserHasNoPinHash_ReturnsFalse()
    {
        var user = TestHelpers.CreateUser();
        // User has no ManagerPinHash set (null by default)
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        var query = new VerifyManagerPinQuery(user.Id, "123456");

        var result = await VerifyManagerPinHandler.HandleAsync(
            query, _userRepository, _passwordHasher, CancellationToken.None);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WrongPin_ReturnsFalse()
    {
        var user = TestHelpers.CreateUser();
        user.SetManagerPinHash("hashed-pin-123456");
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword("wrong-pin", "hashed-pin-123456")
            .Returns(false);

        var query = new VerifyManagerPinQuery(user.Id, "wrong-pin");

        var result = await VerifyManagerPinHandler.HandleAsync(
            query, _userRepository, _passwordHasher, CancellationToken.None);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_CorrectPin_ReturnsTrue()
    {
        var user = TestHelpers.CreateUser();
        user.SetManagerPinHash("hashed-pin-123456");
        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword("123456", "hashed-pin-123456")
            .Returns(true);

        var query = new VerifyManagerPinQuery(user.Id, "123456");

        var result = await VerifyManagerPinHandler.HandleAsync(
            query, _userRepository, _passwordHasher, CancellationToken.None);

        result.IsValid.Should().BeTrue();
    }
}
