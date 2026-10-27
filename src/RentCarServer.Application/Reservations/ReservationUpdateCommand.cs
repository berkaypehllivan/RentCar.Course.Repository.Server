using FluentValidation;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using RentCarServer.Application.Behaviors;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Abstractions;
using RentCarServer.Domain.Branchs;
using RentCarServer.Domain.Customers;
using RentCarServer.Domain.Reservations;
using RentCarServer.Domain.Reservations.ValueObjects;
using RentCarServer.Domain.Shared;
using RentCarServer.Domain.Vehicles;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.Reservations;

[Permission("reservation/edit")]
public sealed record ReservationUpdateCommand(
    Guid Id,
    Guid CustomerId,
    Guid? PickUpLocationId,
    DateOnly PickUpDate,
    TimeOnly PickUpTime,
    DateOnly DeliveryDate,
    TimeOnly DeliveryTime,
    Guid VehicleId,
    decimal VehicleDailyPrice,
    Guid? ProtectionPackageId,
    decimal? ProtectionPackagePrice,
    List<ReservationExtra> ReservationExtras,
    string Note,
    decimal Total,
    int TotalDay
) : IRequest<Result<string>>;



public sealed class ReservationUpdateCommandValidator : AbstractValidator<ReservationUpdateCommand>
{
    public ReservationUpdateCommandValidator()
    {

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Müşteri bilgisi boş olamaz.");

        RuleFor(x => x.PickUpLocationId)
            .NotEmpty().WithMessage("Alış lokasyonu boş olamaz.");

        RuleFor(x => x.VehicleId)
            .NotEmpty().WithMessage("Araç bilgisi boş olamaz.");

        RuleFor(x => x.PickUpDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Alış tarihi bugünden daha eski bir tarih olamaz.");

        RuleFor(x => x.DeliveryDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(x => x.PickUpDate)
            .WithMessage("Teslim tarihi, alış tarihinden önce veya daha eski bir tarih olamaz.");
    }
}

internal sealed class ReservationUpdateCommandHandler(
    IReservationRepository reservationRepository,
    IBranchRepository branchRepository,
    ICustomerRepository customerRepository,
    IVehicleRepository vehicleRepository,
    IUnitOfWork unitOfWork,
    IClaimContext claimContext
    ) : IRequestHandler<ReservationUpdateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ReservationUpdateCommand request, CancellationToken cancellationToken)
    {
        Reservation? reservation = await reservationRepository.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (reservation is null)
            return Result<string>.Failure("Rezervasyon bulunamadı.");

        if (reservation.Status == Status.Completed || reservation.Status == Status.Canceled)
            return Result<string>.Failure("Tamamlanmış veya iptal edilmiş rezervasyonlar güncellenemez.");


        var locationId = request.PickUpLocationId ?? claimContext.GetBranchId();

        #region Şube, Müşteri ve Araç Kontrolleri

        if (reservation.PickUpLocationId.Value != locationId)
        {
            var isBranchExists = await branchRepository.AnyAsync(i => i.Id == locationId, cancellationToken);
            if (!isBranchExists)
                return Result<string>.Failure("Şube bilgisi bulunamadı.");
        }

        if (reservation.CustomerId.Value != request.CustomerId)
        {
            var isCustomerExists = await customerRepository.AnyAsync(i => i.Id == request.CustomerId, cancellationToken);
            if (!isCustomerExists)
                return Result<string>.Failure("Müşteri bilgisi bulunamadı.");
        }

        if (reservation.VehicleId.Value != request.VehicleId)
        {
            var isVehicleExists = await vehicleRepository.AnyAsync(i => i.Id == request.VehicleId, cancellationToken);
            if (!isVehicleExists)
                return Result<string>.Failure("Araç bilgisi bulunamadı.");
        }

        #endregion

        #region Araç Müsaitlik Kontrolü
        if (reservation.PickUpDate.Value != request.PickUpDate || reservation.PickUpTime.Value != request.PickUpTime
            | reservation.DeliveryDate.Value != request.DeliveryDate || reservation.DeliveryTime.Value != request.DeliveryTime)
        {
            var requestedPickUp = request.PickUpDate.ToDateTime(request.PickUpTime);
            var requestedDelivery = request.DeliveryDate.ToDateTime(request.DeliveryTime);

            var possibleOverlaps = await reservationRepository
                .Where(r => r.Id != request.Id && r.VehicleId == request.VehicleId
    && (r.Status.Value == Status.Pending.Value || r.Status.Value == Status.Delivered.Value))
                .Select(s => new
                {
                    Id = s.Id,

                    VehicleId = s.VehicleId,
                    DeliveryDate = s.DeliveryDate.Value,
                    DeliveryTime = s.DeliveryTime.Value,
                    PickUpDate = s.PickUpDate.Value,
                    PickUpTime = s.PickUpTime.Value,
                })
                .ToListAsync(cancellationToken);

            var overlaps = possibleOverlaps.Any(r =>
                requestedPickUp < r.DeliveryDate.ToDateTime(r.DeliveryTime).AddHours(1) &&
                requestedDelivery > r.PickUpDate.ToDateTime(r.PickUpTime)
            );

            if (overlaps)
                return Result<string>.Failure("Seçilen araç, belirtilen tarih ve saat aralığında müsait değil.");
        }

        #endregion

        #region Reservasion Objesinin Oluşturulması

        IdentityId customerId = new(request.CustomerId);
        IdentityId pickUpLocationId = new(locationId);
        PickUpDate pickUpDate = new(request.PickUpDate);
        PickUpTime pickUpTime = new(request.PickUpTime);
        DeliveryDate deliveryDate = new(request.DeliveryDate);
        DeliveryTime deliveryTime = new(request.DeliveryTime);
        IdentityId vehicleId = new(request.VehicleId);
        Price vehicleDailyPrice = new(request.VehicleDailyPrice);

        IdentityId? protectionPackageId = request.ProtectionPackageId is not null ? new(request.ProtectionPackageId.Value) : null;
        Price? protectionPackagePrice = request.ProtectionPackagePrice is not null ? new(request.ProtectionPackagePrice.Value) : null;
        IEnumerable<ReservationExtra> reservationExtras = request.ReservationExtras.Select(s => new ReservationExtra(s.ExtraId, s.Price));
        Note note = new(request.Note);
        Total total = new(request.Total);
        TotalDay totalDay = new(request.TotalDay);
        ReservationHistory history = new("Rezervasyon Güncelendi", "Online Rezervasyon Güncellendi", DateTimeOffset.Now);

        reservation.SetCustomerId(customerId);
        reservation.SetPickUpLocationId(pickUpLocationId);
        reservation.SetPickUpDate(pickUpDate);
        reservation.SetPickUpTime(pickUpTime);
        reservation.SetDeliveryDate(deliveryDate);
        reservation.SetDeliveryTime(deliveryTime);
        reservation.SetVehicleId(vehicleId);
        reservation.SetVehicleDailyPrice(vehicleDailyPrice);

        if (protectionPackageId is not null)
            reservation.SetProtectionPackageId(protectionPackageId);
        if (protectionPackagePrice is not null)
            reservation.SetProtectionPackagePrice(protectionPackagePrice);

        reservation.SetReservationExtras(reservationExtras);
        reservation.SetNote(note);
        reservation.SetTotal(total);
        reservation.SetDeliveryDateTime();
        reservation.SetPickUpDateTime();
        reservation.SetTotalDay(totalDay);
        reservation.SetHistory(history);
        #endregion

        reservationRepository.Update(reservation);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Rezervasyon başarıyla güncellendi.";
    }
}