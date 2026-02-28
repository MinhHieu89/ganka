using Auth.Application.Interfaces;
using Shared.Domain;

namespace Auth.Application.Features;

// --- Command record ---
public sealed record LogoutCommand(Guid UserId);

// --- Handler ---
public sealed class LogoutHandler
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(LogoutCommand command)
    {
        await _refreshTokenRepository.RevokeAllByUserIdAsync(command.UserId, "User logout");
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
