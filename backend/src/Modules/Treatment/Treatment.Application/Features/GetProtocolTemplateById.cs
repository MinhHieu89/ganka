using Shared.Domain;
using Treatment.Application.Interfaces;
using Treatment.Contracts.Dtos;

namespace Treatment.Application.Features;

/// <summary>
/// Query to retrieve a single treatment protocol template by ID.
/// </summary>
public sealed record GetProtocolTemplateByIdQuery(Guid Id);

/// <summary>
/// Wolverine static handler for retrieving a single treatment protocol template by ID.
/// Returns NotFound if the template does not exist.
/// </summary>
public static class GetProtocolTemplateByIdHandler
{
    public static async Task<Result<TreatmentProtocolDto>> Handle(
        GetProtocolTemplateByIdQuery query,
        ITreatmentProtocolRepository repository,
        CancellationToken ct)
    {
        var protocol = await repository.GetByIdAsync(query.Id, ct);
        if (protocol is null)
            return Result.Failure<TreatmentProtocolDto>(Error.NotFound("TreatmentProtocol", query.Id));

        return CreateProtocolTemplateHandler.MapToDto(protocol);
    }
}
