using RentCarServer.Application.Behaviors;
using RentCarServer.Domain.Branchs;
using RentCarServer.Domain.Categories;
using RentCarServer.Domain.Customers;
using RentCarServer.Domain.Extras;
using RentCarServer.Domain.ProtectionPackages;
using RentCarServer.Domain.Reservations;
using RentCarServer.Domain.Vehicles;
using TS.MediatR;

namespace RentCarServer.Application.Reservations;

[Permission("reservation/view")]

public sealed record ReservationGetAllQuery : IRequest<IQueryable<ReservationDto>>;
internal sealed class ReservationGetAllQueryHandler(IReservationRepository repository,
    ICustomerRepository customerRepository,
        IBranchRepository branchRepository,
        IVehicleRepository vehicleRepository,
        ICategoryRepository categoryRepository,
        IProtectionPackageRepository protectionPackageRepository,
        IExtraRepository extraRepository
    ) : IRequestHandler<ReservationGetAllQuery, IQueryable<ReservationDto>>
{
    public Task<IQueryable<ReservationDto>> Handle(ReservationGetAllQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(repository.GetAllWithAudit()
            .MapTo(customerRepository.GetAll(),
            branchRepository.GetAll(),
            vehicleRepository.GetAll(),
            categoryRepository.GetAll(),
            protectionPackageRepository.GetAll(),
            extraRepository.GetAll())
            .AsQueryable());
}