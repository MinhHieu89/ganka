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

public class CreateRoleHandlerTests
{
    private readonly IRoleRepository _roleRepository = Substitute.For<IRoleRepository>();
    private readonly IPermissionRepository _permissionRepository = Substitute.For<IPermissionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IValidator<CreateRoleCommand> _validator = Substitute.For<IValidator<CreateRoleCommand>>();

    private void SetupValidValidator()
    {
        _validator.ValidateAsync(Arg.Any<CreateRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    [Fact]
    public async Task Handle_ValidData_CreatesRoleAndReturnsId()
    {
        // Arrange
        SetupValidValidator();
        var command = new CreateRoleCommand("NewRole", "A new role", []);

        _roleRepository.NameExistsAsync("NewRole", Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var result = await CreateRoleHandler.Handle(
            command, _roleRepository, _permissionRepository, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _roleRepository.Received(1).Add(Arg.Any<Role>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateName_ReturnsFailure()
    {
        // Arrange
        SetupValidValidator();
        var command = new CreateRoleCommand("Admin", "Duplicate", []);

        _roleRepository.NameExistsAsync("Admin", Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await CreateRoleHandler.Handle(
            command, _roleRepository, _permissionRepository, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Conflict");
    }

    [Fact]
    public async Task Handle_WithPermissions_AssignsPermissions()
    {
        // Arrange
        SetupValidValidator();
        var perm1 = TestHelpers.CreatePermission(PermissionModule.Patient, PermissionAction.View);
        var perm2 = TestHelpers.CreatePermission(PermissionModule.Patient, PermissionAction.Create);
        var command = new CreateRoleCommand("CustomRole", "Custom", [perm1.Id, perm2.Id]);

        _roleRepository.NameExistsAsync("CustomRole", Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns(false);
        _permissionRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([perm1, perm2]);

        // Act
        var result = await CreateRoleHandler.Handle(
            command, _roleRepository, _permissionRepository, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _roleRepository.Received(1).Add(Arg.Is<Role>(r => r.RolePermissions.Count == 2));
    }

    [Fact]
    public async Task Handle_ValidationFailure_ReturnsFailure()
    {
        // Arrange
        var failures = new List<ValidationFailure> { new("Name", "Role name is required.") };
        _validator.ValidateAsync(Arg.Any<CreateRoleCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(failures));

        var command = new CreateRoleCommand("", "", []);

        // Act
        var result = await CreateRoleHandler.Handle(
            command, _roleRepository, _permissionRepository, _unitOfWork, _validator, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }
}
