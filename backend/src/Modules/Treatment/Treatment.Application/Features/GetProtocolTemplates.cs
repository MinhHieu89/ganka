using Shared.Domain;
using Treatment.Application.Interfaces;
using Treatment.Contracts.Dtos;
using Treatment.Domain.Enums;

namespace Treatment.Application.Features;

/// <summary>
/// Query to retrieve a list of treatment protocol templates.
/// Optionally filter by TreatmentType and include inactive templates.
/// </summary>
public sealed record GetProtocolTemplatesQuery(
    int? TreatmentType = null,
    bool IncludeInactive = false);

/// <summary>
/// Wolverine static handler for retrieving treatment protocol templates.
/// Supports filtering by type and including inactive templates.
/// </summary>
public static class GetProtocolTemplatesHandler
{
    public static async Task<Result<List<TreatmentProtocolDto>>> Handle(
        GetProtocolTemplatesQuery query,
        ITreatmentProtocolRepository repository,
        CancellationToken ct)
    {
        var protocols = query.TreatmentType.HasValue
            ? await repository.GetByTypeAsync((TreatmentType)query.TreatmentType.Value, query.IncludeInactive, ct)
            : await repository.GetAllAsync(query.IncludeInactive, ct);

        var dtos = protocols.Select(CreateProtocolTemplateHandler.MapToDto).ToList();

        return dtos;
    }
}
