using Auth.Application.Interfaces;
using FluentValidation;
using Shared.Domain;

namespace Auth.Application.Features;

// --- Command record ---
public sealed record UpdateLanguageCommand(Guid UserId, string Language);

// --- Validator ---
public sealed class UpdateLanguageCommandValidator : AbstractValidator<UpdateLanguageCommand>
{
    public UpdateLanguageCommandValidator()
    {
        RuleFor(x => x.Language)
            .Must(lang => lang is "vi" or "en")
            .WithMessage("Language must be 'vi' or 'en'.");
    }
}

// --- Handler ---
public sealed class UpdateLanguageHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateLanguageCommand> _validator;

    public UpdateLanguageHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateLanguageCommand> validator)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<Result> Handle(UpdateLanguageCommand command)
    {
        var validationResult = await _validator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure(Error.Validation(errors));
        }

        var user = await _userRepository.GetByIdAsync(command.UserId);
        if (user is null)
            return Result.Failure(Error.NotFound("User", command.UserId));

        user.SetLanguagePreference(command.Language);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
