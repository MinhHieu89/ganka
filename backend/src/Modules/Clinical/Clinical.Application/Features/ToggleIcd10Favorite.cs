using Clinical.Application.Interfaces;
using Clinical.Contracts.Dtos;
using Clinical.Domain.Entities;
using Shared.Domain;

namespace Clinical.Application.Features;

/// <summary>
/// Wolverine handler for toggling an ICD-10 code as a per-doctor favorite.
/// If already favorited, removes it. If not, adds it.
/// </summary>
public static class ToggleIcd10FavoriteHandler
{
    public static async Task<Result> Handle(
        ToggleIcd10FavoriteCommand command,
        IDoctorIcd10FavoriteRepository favoriteRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var exists = await favoriteRepository.ExistsAsync(command.DoctorId, command.Icd10Code, ct);

        if (exists)
        {
            await favoriteRepository.RemoveAsync(command.DoctorId, command.Icd10Code, ct);
        }
        else
        {
            var favorite = DoctorIcd10Favorite.Create(command.DoctorId, command.Icd10Code);
            await favoriteRepository.AddAsync(favorite, ct);
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
