using Auth.Application.Interfaces;
using Auth.Contracts.Dtos;
using Auth.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Domain;

namespace Auth.Application.Features;

// --- Command record ---
public sealed record LoginCommand(
    string Email,
    string Password,
    bool RememberMe = false,
    string? IpAddress = null);

// --- Validator ---
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");
    }
}

// --- Handler ---
public sealed class LoginHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<LoginCommand> _validator;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IUnitOfWork unitOfWork,
        IValidator<LoginCommand> validator,
        ILogger<LoginHandler> logger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand command)
    {
        var validationResult = await _validator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result<LoginResponse>.Failure(Error.Validation(errors));
        }

        var user = await _userRepository.GetByEmailWithRolesAndPermissionsAsync(command.Email);
        if (user is null)
        {
            _logger.LogWarning("Login attempt for non-existent email: {Email}", command.Email);
            return Result<LoginResponse>.Failure(Error.Unauthorized());
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt for deactivated user: {Email}", command.Email);
            return Result<LoginResponse>.Failure(Error.Unauthorized());
        }

        if (!_passwordHasher.VerifyPassword(command.Password, user.PasswordHash))
        {
            _logger.LogWarning("Invalid password for user: {Email}", command.Email);
            return Result<LoginResponse>.Failure(Error.Unauthorized());
        }

        // Generate tokens
        var permissions = user.GetEffectivePermissions()
            .Select(p => p.ToString())
            .ToList();

        var (accessToken, expiresAt) = _jwtService.GenerateAccessToken(user, permissions);
        var refreshTokenValue = _jwtService.GenerateRefreshToken();

        // Store refresh token with family ID
        var familyId = Guid.NewGuid();
        var refreshTokenLifetimeDays = _jwtService.GetRefreshTokenLifetimeDays(command.RememberMe);
        var refreshToken = new RefreshToken(
            refreshTokenValue,
            user.Id,
            DateTime.UtcNow.AddDays(refreshTokenLifetimeDays),
            familyId);

        _refreshTokenRepository.Add(refreshToken);

        // Record login event on the aggregate
        user.RecordLogin(command.IpAddress);

        await _unitOfWork.SaveChangesAsync();

        var userDto = new UserDto(
            user.Id,
            user.Email,
            user.FullName,
            user.PreferredLanguage,
            user.IsActive,
            user.UserRoles.Select(ur => ur.Role.Name).ToList(),
            permissions);

        return new LoginResponse(accessToken, refreshTokenValue, expiresAt, userDto);
    }
}
