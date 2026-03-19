using Clinical.Contracts.IntegrationEvents;
using FluentValidation;
using Shared.Domain;
using Treatment.Application.Features;
using Wolverine;

namespace Treatment.Unit.Tests.Features;

/// <summary>
/// Tests for the DB-backed OSDI token registration flow.
/// </summary>
public class RegisterOsdiTokenHandlerTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();
    private readonly IValidator<RegisterOsdiTokenCommand> _validator = new RegisterOsdiTokenCommandValidator();

    [Fact]
    public async Task Handle_ValidCommand_InvokesCreateOsdiTokenForTreatmentCommand()
    {
        // Arrange
        var token = "test-token-123";
        var command = new RegisterOsdiTokenCommand(Guid.NewGuid(), 1, token);
        var expectedResponse = new CreateOsdiTokenForTreatmentResponse(
            token, $"/osdi/{token}", DateTime.UtcNow.AddHours(24));

        _bus.InvokeAsync<CreateOsdiTokenForTreatmentResponse>(
            Arg.Any<CreateOsdiTokenForTreatmentCommand>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var result = await RegisterOsdiTokenHandler.Handle(command, _bus, _validator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().Be(token);
        result.Value.Url.Should().Be($"/osdi/{token}");
        result.Value.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(24), TimeSpan.FromMinutes(1));

        await _bus.Received(1).InvokeAsync<CreateOsdiTokenForTreatmentResponse>(
            Arg.Is<CreateOsdiTokenForTreatmentCommand>(c => c.Token == token),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmptyToken_ReturnsValidationError()
    {
        // Arrange
        var command = new RegisterOsdiTokenCommand(Guid.NewGuid(), 1, "");

        // Act
        var result = await RegisterOsdiTokenHandler.Handle(command, _bus, _validator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        await _bus.DidNotReceive().InvokeAsync<CreateOsdiTokenForTreatmentResponse>(
            Arg.Any<CreateOsdiTokenForTreatmentCommand>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmptyPackageId_ReturnsValidationError()
    {
        // Arrange
        var command = new RegisterOsdiTokenCommand(Guid.Empty, 1, "some-token");

        // Act
        var result = await RegisterOsdiTokenHandler.Handle(command, _bus, _validator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ResponseContainsUrlFromClinicalModule()
    {
        // Arrange
        var token = "abc-def-ghi";
        var command = new RegisterOsdiTokenCommand(Guid.NewGuid(), null, token);
        var clinicalResponse = new CreateOsdiTokenForTreatmentResponse(
            token, "/osdi/abc-def-ghi", DateTime.UtcNow.AddHours(24));

        _bus.InvokeAsync<CreateOsdiTokenForTreatmentResponse>(
            Arg.Any<CreateOsdiTokenForTreatmentCommand>(),
            Arg.Any<CancellationToken>())
            .Returns(clinicalResponse);

        // Act
        var result = await RegisterOsdiTokenHandler.Handle(command, _bus, _validator, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Url.Should().Be("/osdi/abc-def-ghi");
    }
}
