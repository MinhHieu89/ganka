using Auth.Application.Features;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;

namespace Auth.Unit.Tests.Features;

public class UpdateLanguageHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IValidator<UpdateLanguageCommand> _validator = Substitute.For<IValidator<UpdateLanguageCommand>>();

    private UpdateLanguageHandler CreateSut() => new(_userRepository, _unitOfWork, _validator);

    private void SetupValidValidator()
    {
        _validator.ValidateAsync(Arg.Any<UpdateLanguageCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
    }

    [Fact]
    public async Task Handle_ValidLanguage_UpdatesAndSaves()
    {
        // Arrange
        SetupValidValidator();
        var user = TestHelpers.CreateUser("user@test.com");
        var command = new UpdateLanguageCommand(user.Id, "en");

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        var sut = CreateSut();

        // Act
        var result = await sut.Handle(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.PreferredLanguage.Should().Be("en");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InvalidLanguage_ReturnsValidationFailure()
    {
        // Arrange
        var failures = new List<ValidationFailure>
        {
            new("Language", "Language must be 'vi' or 'en'.")
        };
        _validator.ValidateAsync(Arg.Any<UpdateLanguageCommand>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(failures));

        var command = new UpdateLanguageCommand(Guid.NewGuid(), "fr");
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task Handle_NonExistentUser_ReturnsFailure()
    {
        // Arrange
        SetupValidValidator();
        var userId = Guid.NewGuid();
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var command = new UpdateLanguageCommand(userId, "vi");
        var sut = CreateSut();

        // Act
        var result = await sut.Handle(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
