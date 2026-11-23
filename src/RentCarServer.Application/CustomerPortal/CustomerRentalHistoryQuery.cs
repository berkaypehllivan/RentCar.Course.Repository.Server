using Microsoft.EntityFrameworkCore;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Abstractions;
using RentCarServer.Domain.Branchs;
using RentCarServer.Domain.Reservations;
using RentCarServer.Domain.Reservations.ValueObjects;
using RentCarServer.Domain.Vehicles;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.CustomerPortal;

public sealed record CustomerRentalHistoryQuery() : IRequest<Result<CustomerRentalHistoryResponse>>;

public sealed record CustomerRentalHistoryResponse(
    CustomerRentalStatsDto Stats,
    List<CustomerRentalItemDto> Rentals);

public sealed record CustomerRentalStatsDto(
    int TotalRentals,
    int ActiveRentals,
    int TotalDays,
    decimal TotalSpent);

public sealed record CustomerRentalItemDto(
    Guid Id,
    string ReservationNumber,
    string Status,
    DateOnly PickUpDate,
    TimeOnly PickUpTime,
    DateOnly DeliveryDate,
    TimeOnly DeliveryTime,
    int TotalDay,
    decimal Total,
    string PickUpLocationName,
    string VehicleBrand,
    string VehicleModel,
    string VehicleImageUrl,
    string VehicleTransmission,
    string VehicleFuelType,
    int VehicleSeatCount,
    DateTimeOffset CreatedAt);

internal sealed class CustomerRentalHistoryQueryHandler(
    IReservationRepository reservationRepository,
    IVehicleRepository vehicleRepository,
    IBranchRepository branchRepository,
    IClaimContext claimContext) : IRequestHandler<CustomerRentalHistoryQuery, Result<CustomerRentalHistoryResponse>>
{
    public async Task<Result<CustomerRentalHistoryResponse>> Handle(CustomerRentalHistoryQuery request, CancellationToken cancellationToken)
    {
        var customerId = new IdentityId(claimContext.GetUserId());

        var rentals = await (
            from r in reservationRepository.GetAll()
            join v in vehicleRepository.GetAll() on r.VehicleId equals v.Id
            join b in branchRepository.GetAll() on r.PickUpLocationId equals b.Id
            where r.CustomerId == customerId
            orderby r.CreatedAt descending
            select new CustomerRentalItemDto(
                r.Id,
                r.ReservationNumber.Value,
                r.Status.Value,
                r.PickUpDate.Value,
                r.PickUpTime.Value,
                r.DeliveryDate.Value,
                r.DeliveryTime.Value,
                r.TotalDay.Value,
                r.Total.Value,
                b.Name.Value,
                v.Brand.Value,
                v.Model.Value,
                v.ImageUrl.Value,
                v.Transmission.Value,
                v.FuelType.Value,
                v.SeatCount.Value,
                r.CreatedAt))
                .ToListAsync(cancellationToken);

        var nonCancelledRentals = rentals
            .Where(x => x.Status != Status.Canceled.Value)
            .ToList();

        var activeCount = rentals.Count(x =>
            x.Status == Status.Pending.Value ||
            x.Status == Status.Delivered.Value);

        var stats = new CustomerRentalStatsDto(
            TotalRentals: rentals.Count,
            ActiveRentals: activeCount,
            TotalDays: nonCancelledRentals.Sum(x => x.TotalDay),
            TotalSpent: nonCancelledRentals.Sum(x => x.Total));

        return new CustomerRentalHistoryResponse(stats, rentals);

    }
}