using Microsoft.EntityFrameworkCore;
using RentCarServer.Domain.Vehicles;
using TS.MediatR;

namespace RentCarServer.Application.Vehicles;

public sealed record VehicleGetColorsQuery() : IRequest<List<string>>;

internal sealed class VehicleGetColorsQueryHandler(IVehicleRepository repository)
    : IRequestHandler<VehicleGetColorsQuery, List<string>>
{
    public async Task<List<string>> Handle(VehicleGetColorsQuery request, CancellationToken cancellationToken)
    {
        return await repository.GetAll()
            .Select(v => v.Color.Value)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
