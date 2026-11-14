using Microsoft.EntityFrameworkCore;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Branchs;
using RentCarServer.Domain.Vehicles;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.Dashboards;

public sealed record DashboardBranchVehicleCountsQuery : IRequest<Result<List<DashboardBranchVehicleCountDto>>>;

public sealed record DashboardBranchVehicleCountDto(
    Guid BranchId,
    string BranchName,
    int VehicleCount);

internal sealed class DashboardBranchVehicleCountsQueryHandler(IVehicleRepository vehicleRepository,
    IBranchRepository branchRepository, IClaimContext claimContext) : IRequestHandler<DashboardBranchVehicleCountsQuery, Result<List<DashboardBranchVehicleCountDto>>>
{
    public async Task<Result<List<DashboardBranchVehicleCountDto>>> Handle(DashboardBranchVehicleCountsQuery request, CancellationToken cancellationToken)
    {
        var vehicles = vehicleRepository.GetAll();
        var branchesQuery = branchRepository.GetAll();

        if (!claimContext.IsSysAdmin())
        {
            var branchId = claimContext.GetBranchId();
            vehicles = vehicles.Where(p => p.BranchId.Value == branchId);
            branchesQuery = branchesQuery.Where(p => p.Id == branchId);
        }

        var vehicleCounts = await vehicles
            .GroupBy(p => p.BranchId.Value)
            .Select(g => new { BranchId = g.Key, Count = g.Count() })
            .ToListAsync();

        var branches = await branchesQuery
            .Select(p => new { Id = p.Id.Value, Name = p.Name.Value })
            .ToListAsync(cancellationToken);

        var data = branches
            .Select(b => new DashboardBranchVehicleCountDto(
                b.Id,
                b.Name,
                vehicleCounts.FirstOrDefault(v => v.BranchId == b.Id)?.Count ?? 0))
            .OrderBy(p => p.BranchName)
            .ToList();

        return Result<List<DashboardBranchVehicleCountDto>>.Succeed(data);
    }
}