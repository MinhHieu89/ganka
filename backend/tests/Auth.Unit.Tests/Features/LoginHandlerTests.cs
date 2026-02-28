using Auth.Application.Features;
using Auth.Application.Interfaces;
using Auth.Contracts.Dtos;
using Auth.Domain.Entities;
using Auth.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Domain;

namespace Auth.Unit.Tests.Features;

public class LoginHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtService _jwtService = Substitute.For<IJwtService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IValidator<LoginCommand> _validator = Substitute.For<IValidator<LoginCommand>>();
    private readonly ILogger<LoginHandler> _logger = Substitute.For<ILogger<LoginHandler>>();

    private LoginHandler CreateSut() => new(
        _userRepository,
        _refreshTokenRepository,
        _passwordHasher,
        _jwtService,
        _unitOfWork,
        _validator,
        _logger);

    private void SetupValidValidator()
    {
        _validator.ValidateAsync(Arg.Any<LoginCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    private User CreateTestUserWithRolesAndPermissions()
    {
        var permission = TestHelpers.CreatePermission(PermissionModule.Auth, PermissionAction.Manage, "Manage Auth");
        var role = TestHelpers.CreateRole("Admin", "Admin Role", true, permission);
        return TestHelpers.CreateFullyWiredUser(
            "user@test.com", "Test User", "hashed-pw", true,
            (role, [permission]));
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsLoginResponse()
    {
        // Arrange
        SetupValidValidator();
        var user = CreateTestUserWithRolesAndPermissions();
        var command = new LoginCommand("user@test.com", "Password123!");

        _userRepository.GetByEmailWithRolesAndPermissionsAsync("user@test.com", Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword("Password123!", user.PasswordHash).Returns(true);
        _jwtService.GenerateAccessToken(user, Arg.Any<IEnumerable<string>>())
            .Returns(("access-token-value", DateTime.UtcNow.AddHours(1)));
        _jwtService.GenerateRefreshToken().Returns("refresh-token-value");
        _jwtService.GetRefreshTokenLifetimeDays(false).Returns(7);

        var sut = CreateSut();

        // Act
        var result = await sut.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access-token-value");
        result.Value.RefreshToken.Should().Be("refresh-token-value");
        result.Value.User.Should().NotBeNull();
        result.Value.User.Email.Should().Be("user@test.com");
    }

    [Fact]
    public async Task Handle_NonExistentUser_ReturnsUnauthorized()
    {
        // Arrange
        SetupValidValidator();
        var command = new LoginCommand("noone@test.com", "Password123!");

        _userRepository.GetByEmailWithRolesAndPermissionsAsync("noone@test.com", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var sut = CreateSut();

        // Act
        var result = await sut.Handle(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Unauthorized");
    }

    [Fact]
    public async Task Handle_InactiveUser_ReturnsUnauthorized()
    {
        // Arrange
        SetupValidValidator();
        var user = TestHelpers.CreateUser("inactive@test.com", "Inactive User", "hash", isActive: false);
        var command = new LoginCommand("inactive@test.com", "Password123!");

        _userRepository.GetByEmailWithRolesAndPermissionsAsync("inactive@test.com", Arg.Any<CancellationToken>())
            .Returns(user);

        var sut = CreateSut();

        // Act
        var result = await sut.Handle(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Unauthorized");
    }

    [Fact]
    public async Task Handle_WrongPassword_ReturnsUnauthorized()
    {
        // Arrange
        SetupValidValidator();
        var user = CreateTestUserWithRolesAndPermissions();
        var command = new LoginCommand("user@test.com", "WrongPassword!");

        _userRepository.GetByEmailWithRolesAndPermissionsAsync("user@test.com", Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword("WrongPassword!", user.PasswordHash).Returns(false);

        var sut = CreateSut();

        // Act
        var result = await sut.Handle(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Unauthorized");
    }

    [Fact]
    public async Task Handle_ValidLogin_PersistsRefreshToken()
    {
        // Arrange
        SetupValidValidator();
        var user = CreateTestUserWithRolesAndPermissions();
        var command = new LoginCommand("user@test.com", "Password123!");

        _userRepository.GetByEmailWithRolesAndPermissionsAsync("user@test.com", Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword("Password123!", user.PasswordHash).Returns(true);
        _jwtService.GenerateAccessToken(user, Arg.Any<IEnumerable<string>>())
            .Returns(("access-token", DateTime.UtcNow.AddHours(1)));
        _jwtService.GenerateRefreshToken().Returns("refresh-token");
        _jwtService.GetRefreshTokenLifetimeDays(false).Returns(7);

        var sut = CreateSut();

        // Act
        await sut.Handle(command);

        // Assert
        _refreshTokenRepository.Received(1).Add(Arg.Any<RefreshToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidationFailure_ReturnsFailure()
    {
        // Arrange
        var failures = new List<ValidationFailure>
        {
            new("Email", "Email is required.")
        };
        _validator.ValidateAsync(Arg.Any<LoginCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(failures));

        var command = new LoginCommand("", "");

        var sut = CreateSut();

        // Act
        var result = await sut.Handle(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }
}
