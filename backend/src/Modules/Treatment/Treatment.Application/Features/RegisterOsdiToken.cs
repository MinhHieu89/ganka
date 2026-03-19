using Clinical.Contracts.IntegrationEvents;
using FluentValidation;
using Shared.Domain;
using Wolverine;

namespace Treatment.Application.Features;

/// <summary>
/// Command to register an OSDI self-fill QR token linked to a treatment package/session.
/// The token is persisted in the Clinical module's database via cross-module command.
/// </summary>
public sealed record RegisterOsdiTokenCommand(
    Guid PackageId,
    int? SessionNumber,
    string Token);

/// <summary>
/// Response containing the registered token, URL, and expiry for frontend.
/// </summary>
public sealed record RegisterOsdiTokenResponse(string Token, string Url, DateTime ExpiresAt);

/// <summary>
/// Validator for <see cref="RegisterOsdiTokenCommand"/>.
/// </summary>
public class RegisterOsdiTokenCommandValidator : AbstractValidator<RegisterOsdiTokenCommand>
{
    public RegisterOsdiTokenCommandValidator()
    {
        RuleFor(x => x.PackageId).NotEmpty();
        RuleFor(x => x.Token).NotEmpty().WithMessage("Token is required.");
    }
}

/// <summary>
/// Wolverine static handler for <see cref="RegisterOsdiTokenCommand"/>.
/// Delegates to Clinical module via IMessageBus to create a DB-backed OsdiSubmission record.
/// </summary>
public static class RegisterOsdiTokenHandler
{
    public static async Task<Result<RegisterOsdiTokenResponse>> Handle(
        RegisterOsdiTokenCommand command,
        IMessageBus bus,
        IValidator<RegisterOsdiTokenCommand> validator,
        CancellationToken ct)
    {
        var validationResult = validator.Validate(command);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result<RegisterOsdiTokenResponse>.Failure(Error.ValidationWithDetails(errors));
        }

        // Create DB-backed token via Clinical module's handler
        var clinicalResponse = await bus.InvokeAsync<CreateOsdiTokenForTreatmentResponse>(
            new CreateOsdiTokenForTreatmentCommand(command.Token), ct);

        var response = new RegisterOsdiTokenResponse(
            clinicalResponse.Token,
            clinicalResponse.Url,
            clinicalResponse.ExpiresAt);

        return Result<RegisterOsdiTokenResponse>.Success(response);
    }
}
