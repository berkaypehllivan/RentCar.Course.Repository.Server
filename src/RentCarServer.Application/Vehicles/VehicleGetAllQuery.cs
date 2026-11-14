using RentCarServer.Application.Behaviors;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Branchs;
using RentCarServer.Domain.Categories;
using RentCarServer.Domain.Vehicles;
using TS.MediatR;

namespace RentCarServer.Application.Vehicles;

[Permission("vehicle:view")]
public sealed record VehicleGetAllQuery : IRequest<IQueryable<VehicleDto>>;

internal sealed class VehicleGetAllQueryHandler(
    IVehicleRepository vehicleRepository,
    IBranchRepository branchRepository,
    ICategoryRepository categoryRepository,
    IClaimContext claimContext)
    : IRequestHandler<VehicleGetAllQuery, IQueryable<VehicleDto>>
{
    public Task<IQueryable<VehicleDto>> Handle(VehicleGetAllQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(
            vehicleRepository
                .GetAllWithAudit()
            .ForCurrentBranch(claimContext)
                .MapTo(
                branchRepository.GetAll(),
                categoryRepository.GetAll())
                .AsQueryable()
        );
}