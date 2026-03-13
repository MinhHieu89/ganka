using Optical.Application.Interfaces;
using Optical.Contracts.Dtos;
using Optical.Domain.Entities;
using Shared.Domain;

namespace Optical.Application.Features.Frames;

/// <summary>
/// Query to retrieve paginated list of frames.
/// </summary>
public sealed record GetFramesQuery(bool IncludeInactive = false, int Page = 1, int PageSize = 20);

/// <summary>
/// Paginated result for frame list queries.
/// </summary>
public sealed record PagedFramesResult(List<FrameSummaryDto> Items, int TotalCount, int Page, int PageSize);

/// <summary>
/// Wolverine static handler for retrieving all frames with optional inactive filter.
/// Maps Frame entities to FrameSummaryDto inline (no AutoMapper).
/// </summary>
public static class GetFramesHandler
{
    public static async Task<Result<PagedFramesResult>> Handle(
        GetFramesQuery query,
        IFrameRepository repository,
        CancellationToken ct)
    {
        var frames = await repository.GetAllAsync(query.IncludeInactive, ct);

        var totalCount = frames.Count;
        var paged = frames
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(ToSummaryDto)
            .ToList();

        return new PagedFramesResult(
            Items: paged,
            TotalCount: totalCount,
            Page: query.Page,
            PageSize: query.PageSize);
    }

    internal static FrameSummaryDto ToSummaryDto(Frame frame) =>
        new(
            Id: frame.Id,
            Brand: frame.Brand,
            Model: frame.Model,
            Color: frame.Color,
            SizeDisplay: frame.SizeDisplay,
            Material: (int)frame.Material,
            FrameType: (int)frame.Type,
            Gender: (int)frame.Gender,
            SellingPrice: frame.SellingPrice,
            Barcode: frame.Barcode,
            StockQuantity: frame.StockQuantity,
            IsActive: frame.IsActive);
}
