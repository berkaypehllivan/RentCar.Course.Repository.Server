using Microsoft.EntityFrameworkCore;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Vehicles;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.Dashboards;

public sealed record DashboardTotalVehicleCountQuery : IRequest<Result<int>>;

internal sealed class DashboardTotalVehicleCountQueryHandler(IVehicleRepository vehicleRepository, IClaimContext claimContext) : IRequestHandler<DashboardTotalVehicleCountQuery, Result<int>>
{
    public async Task<Result<int>> Handle(DashboardTotalVehicleCountQuery request, CancellationToken cancellationToken)
    {
        var branchId = claimContext.GetBranchId();

        return await vehicleRepository
            .Where(p => p.BranchId.Value == branchId)
            .CountAsync(cancellationToken);
    }
}