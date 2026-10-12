using Microsoft.EntityFrameworkCore;
using RentCarServer.Domain.Vehicles;
using TS.MediatR;

namespace RentCarServer.Application.Vehicles;

public sealed record VehicleGetYearsQuery() : IRequest<List<int>>;

internal sealed class VehicleGetYearsQueryHandler(IVehicleRepository repository)
    : IRequestHandler<VehicleGetYearsQuery, List<int>>
{
    public async Task<List<int>> Handle(VehicleGetYearsQuery request, CancellationToken cancellationToken)
    {
        return await repository.GetAll()
            .Select(x => x.ModelYear.Value)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
