using FluentValidation;
using Shared.Domain;

namespace Treatment.Application.Features;

/// <summary>
/// Command to register an OSDI self-fill QR token linked to a treatment package/session.
/// The token is stored in-memory with a 24-hour TTL.
/// </summary>
public sealed record RegisterOsdiTokenCommand(
    Guid PackageId,
    int? SessionNumber,
    string Token);

/// <summary>
/// Response containing the registered token for frontend confirmation.
/// </summary>
public sealed record RegisterOsdiTokenResponse(string Token, DateTime ExpiresAt);

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
/// In-memory record linking a QR token to its package/session context.
/// </summary>
public sealed record OsdiTokenInfo(
    Guid PackageId,
    int? SessionNumber,
    DateTime ExpiresAt);

/// <summary>
/// Wolverine static handler for <see cref="RegisterOsdiTokenCommand"/>.
/// Stores the token in the <see cref="IOsdiTokenStore"/> singleton with 24-hour TTL.
/// </summary>
public static class RegisterOsdiTokenHandler
{
    public static Task<Result<RegisterOsdiTokenResponse>> Handle(
        RegisterOsdiTokenCommand command,
        IOsdiTokenStore tokenStore,
        IValidator<RegisterOsdiTokenCommand> validator,
        CancellationToken ct)
    {
        var validationResult = validator.Validate(command);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Task.FromResult(
                Result<RegisterOsdiTokenResponse>.Failure(Error.ValidationWithDetails(errors)));
        }

        var expiresAt = DateTime.UtcNow.AddHours(24);
        var tokenInfo = new OsdiTokenInfo(command.PackageId, command.SessionNumber, expiresAt);
        tokenStore.Register(command.Token, tokenInfo);

        var response = new RegisterOsdiTokenResponse(command.Token, expiresAt);
        return Task.FromResult(Result<RegisterOsdiTokenResponse>.Success(response));
    }
}
