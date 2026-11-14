using Microsoft.EntityFrameworkCore;
using RentCarServer.Domain.Vehicles;
using TS.MediatR;

namespace RentCarServer.Application.Vehicles;

public sealed record VehicleGetBrandsQuery() : IRequest<List<string>>;

internal sealed class VehicleGetBrandsQueryHandler(IVehicleRepository repository)
    : IRequestHandler<VehicleGetBrandsQuery, List<string>>
{
    public async Task<List<string>> Handle(VehicleGetBrandsQuery request, CancellationToken cancellationToken)
    {
        return await repository.GetAll()
            .Select(x => x.Brand.Value)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
