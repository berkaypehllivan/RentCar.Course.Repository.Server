using Microsoft.EntityFrameworkCore;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Abstractions;
using RentCarServer.Domain.Branchs;
using RentCarServer.Domain.Customers;
using RentCarServer.Domain.Extras;
using RentCarServer.Domain.ProtectionPackages;
using RentCarServer.Domain.Reservations;
using RentCarServer.Domain.Vehicles;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.CustomerPortal;

public sealed record CustomerRentalDetailQuery(Guid Id) : IRequest<Result<CustomerRentalDetailDto>>;

public sealed record CustomerRentalDetailDto(
    Guid Id,
    string ReservationNumber,
    string Status,
    DateOnly PickUpDate,
    TimeOnly PickUpTime,
    DateOnly DeliveryDate,
    TimeOnly DeliveryTime,
    int TotalDay,
    decimal VehicleDailyPrice,
    decimal VehicleTotalPrice,
    string ProtectionPackageName,
    decimal ProtectionPackagePrice,
    List<CustomerRentalExtraDto> Extras,
    decimal ExtrasTotalPrice,
    decimal Total,
    string? Note,
    DateTimeOffset CreatedAt,
    CustomerRentalVehicleDto Vehicle,
    CustomerRentalBranchDto Branch,
    CustomerRentalDriverDto Driver,
    CustomerRentalPaymentDto Payment);

public sealed record CustomerRentalPaymentDto(
    string MaskedCardNumber,
    string CardOwner);

public sealed record CustomerRentalDriverDto(
    string FullName,
    string MaskedIdentityNumber,
    DateOnly DateOfBirth,
    string Email,
    string PhoneNumber);

public sealed record CustomerRentalBranchDto(
    string Name,
    string FullAddress,
    string PhoneNumber);


public sealed record CustomerRentalVehicleDto(
    string Brand,
    string Model,
    int ModelYear,
    string Plate,
    string ImageUrl,
    string Transmission,
    string FuelType,
    int SeatCount);


public sealed record CustomerRentalExtraDto(
    string Name,
    decimal Price);

internal sealed class CustomerRentalDetailQueryHandler(
    IReservationRepository reservationRepository,
    IVehicleRepository vehicleRepository,
    IBranchRepository branchRepository,
    ICustomerRepository customerRepository,
    IProtectionPackageRepository protectionPackageRepository,
    IExtraRepository extraRepository,
    IClaimContext claimContext
    ) : IRequestHandler<CustomerRentalDetailQuery, Result<CustomerRentalDetailDto>>
{
    public async Task<Result<CustomerRentalDetailDto>> Handle(CustomerRentalDetailQuery request, CancellationToken cancellationToken)
    {
        var customerId = new IdentityId(claimContext.GetUserId());

        var reservation = await reservationRepository
            .FirstOrDefaultAsync(
            r => r.Id == request.Id && r.CustomerId == customerId, cancellationToken);

        if (reservation is null)
            return Result<CustomerRentalDetailDto>.Failure("Rezervasyon bulunamadı.");

        var vehicle = await vehicleRepository
            .FirstOrDefaultAsync(v => v.Id == reservation.VehicleId, cancellationToken);

        var branch = await branchRepository
            .FirstOrDefaultAsync(b => b.Id == reservation.PickUpLocationId, cancellationToken);

        var customer = await customerRepository
            .FirstOrDefaultAsync(c => c.Id == reservation.CustomerId, cancellationToken);

        if (vehicle is null || branch is null || customer is null)
            return Result<CustomerRentalDetailDto>.Failure("Rezervasyona ait ilişkili kayıtlar eksik.");

        string protectionPackageName = "";
        if (reservation.ProtectionPackageId is not null)
        {
            var package = await protectionPackageRepository
                .FirstOrDefaultAsync(p => p.Id == reservation.ProtectionPackageId, cancellationToken);
            protectionPackageName = package.Name.Value;
        }

        var extraIds = reservation.ReservationExtras.Select(e => e.ExtraId).ToList();

        var extrasLookup = extraIds.Count > 0
            ? await extraRepository
                .Where(e => extraIds.Contains(e.Id))
                .ToDictionaryAsync(e => e.Id.Value, e => e.Name.Value, cancellationToken)
            : new Dictionary<Guid, string>();

        var extras = reservation.ReservationExtras
            .Select(e => new CustomerRentalExtraDto(
                extrasLookup.GetValueOrDefault(e.ExtraId, "Ek Hizmet"),
                e.Price))
            .ToList();

        var vehicleTotalPrice = reservation.VehicleDailyPrice.Value * reservation.TotalDay.Value;
        var extrasTotalPrice = extras.Sum(e => e.Price);

        return new CustomerRentalDetailDto(
            reservation.Id,
            reservation.ReservationNumber.Value,
            reservation.Status.Value,
            reservation.PickUpDate.Value,
            reservation.PickUpTime.Value,
            reservation.DeliveryDate.Value,
            reservation.DeliveryTime.Value,
            reservation.TotalDay.Value,
            reservation.VehicleDailyPrice.Value,
            vehicleTotalPrice,
            protectionPackageName,
            reservation.ProtectionPackagePrice.Value,
            extras,
            extrasTotalPrice,
            reservation.Total.Value,
            reservation.Note?.Value,
            reservation.CreatedAt,
            new CustomerRentalVehicleDto(
                vehicle.Brand.Value,
                vehicle.Model.Value,
                vehicle.ModelYear.Value,
                vehicle.Plate.Value,
                vehicle.ImageUrl.Value,
                vehicle.Transmission.Value,
                vehicle.FuelType.Value,
                vehicle.SeatCount.Value),
            new CustomerRentalBranchDto(
                branch.Name.Value,
                branch.Address.FullAddress,
                branch.Contact.PhoneNumber1),
            new CustomerRentalDriverDto(
                $"{customer.FirstName.Value} {customer.LastName.Value}",
                MaskIdentityNumber(customer.IdentityNumber.Value),
                customer.DateOfBirth.Value,
                customer.Email.Value,
                customer.PhoneNumber.Value),
            new CustomerRentalPaymentDto(
                MaskCardNumber(reservation.PaymentInformation.CardNumber),
                reservation.PaymentInformation.CardOwner));
    }

    private static string MaskIdentityNumber(string tc)
    {
        if (string.IsNullOrWhiteSpace(tc) || tc.Length < 7) return tc;
        return $"{tc[..3]}****{tc[^4..]}";
    }

    private static string MaskCardNumber(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber)) return "****";
        var digits = new string(cardNumber.Where(char.IsDigit).ToArray());
        if (digits.Length < 4) return "****";
        return $"**** **** **** {digits[^4..]}";
    }
}