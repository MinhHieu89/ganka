using Auth.Application.Features;
using Auth.Application.Interfaces;
using Auth.Contracts.Dtos;
using Auth.Domain.Entities;
using Auth.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;

namespace Auth.Unit.Tests.Features;

public class CreateUserHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IRoleRepository _roleRepository = Substitute.For<IRoleRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IValidator<CreateUserCommand> _validator = Substitute.For<IValidator<CreateUserCommand>>();

    private void SetupValidValidator()
    {
        _validator.ValidateAsync(Arg.Any<CreateUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    [Fact]
    public async Task Handle_ValidData_CreatesUserAndReturnsId()
    {
        // Arrange
        SetupValidValidator();
        var role = TestHelpers.CreateRole("Doctor");
        var command = new CreateUserCommand("new@test.com", "New User", "Password123!", [role.Id]);

        _userRepository.EmailExistsAsync("new@test.com", Arg.Any<CancellationToken>()).Returns(false);
        _roleRepository.GetByIdAsync(role.Id, Arg.Any<CancellationToken>()).Returns(role);
        _passwordHasher.HashPassword("Password123!").Returns("hashed-password");

        // Act
        var result = await CreateUserHandler.Handle(
            command, _userRepository, _roleRepository, _passwordHasher, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _userRepository.Received(1).Add(Arg.Any<User>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsFailure()
    {
        // Arrange
        SetupValidValidator();
        var command = new CreateUserCommand("existing@test.com", "Dup User", "Password123!", [Guid.NewGuid()]);

        _userRepository.EmailExistsAsync("existing@test.com", Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await CreateUserHandler.Handle(
            command, _userRepository, _roleRepository, _passwordHasher, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Conflict");
    }

    [Fact]
    public async Task Handle_ValidationFailure_ReturnsFailure()
    {
        // Arrange
        var failures = new List<ValidationFailure> { new("Email", "Email is required.") };
        _validator.ValidateAsync(Arg.Any<CreateUserCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(failures));

        var command = new CreateUserCommand("", "", "", []);

        // Act
        var result = await CreateUserHandler.Handle(
            command, _userRepository, _roleRepository, _passwordHasher, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Handle_WithRoles_AssignsRolesToUser()
    {
        // Arrange
        SetupValidValidator();
        var role1 = TestHelpers.CreateRole("Doctor");
        var role2 = TestHelpers.CreateRole("Nurse");
        var command = new CreateUserCommand("user@test.com", "User", "Password123!", [role1.Id, role2.Id]);

        _userRepository.EmailExistsAsync("user@test.com", Arg.Any<CancellationToken>()).Returns(false);
        _roleRepository.GetByIdAsync(role1.Id, Arg.Any<CancellationToken>()).Returns(role1);
        _roleRepository.GetByIdAsync(role2.Id, Arg.Any<CancellationToken>()).Returns(role2);
        _passwordHasher.HashPassword("Password123!").Returns("hashed");

        // Act
        var result = await CreateUserHandler.Handle(
            command, _userRepository, _roleRepository, _passwordHasher, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _userRepository.Received(1).Add(Arg.Is<User>(u => u.UserRoles.Count == 2));
    }
}
