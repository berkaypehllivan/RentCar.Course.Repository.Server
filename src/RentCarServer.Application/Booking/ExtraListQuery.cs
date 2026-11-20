using Microsoft.EntityFrameworkCore;
using RentCarServer.Domain.Extras;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.Booking;

public sealed record ExtraListQuery : IRequest<Result<List<ExtraListDto>>>;

public sealed record ExtraListDto(
    Guid Id,
    string Name,
    decimal Price,
    string Description);

internal sealed class ExtraListQueryHandler(IExtraRepository extraRepository) : IRequestHandler<ExtraListQuery, Result<List<ExtraListDto>>>
{
    public async Task<Result<List<ExtraListDto>>> Handle(ExtraListQuery request, CancellationToken cancellationToken)
    {
        var extras = await extraRepository
            .Where(e => e.IsActive)
            .Select(e => new ExtraListDto(e.Id, e.Name.Value, e.Price.Value, e.Description.Value))
            .ToListAsync(cancellationToken);

        return extras;
    }
}