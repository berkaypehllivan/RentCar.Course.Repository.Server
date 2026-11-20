using FluentValidation;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using RentCarServer.Application.Behaviors;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Abstractions;
using RentCarServer.Domain.Branchs;
using RentCarServer.Domain.Customers;
using RentCarServer.Domain.Reservations;
using RentCarServer.Domain.Reservations.Forms;
using RentCarServer.Domain.Reservations.Forms.ValueObjects;
using RentCarServer.Domain.Reservations.ValueObjects;
using RentCarServer.Domain.Shared;
using RentCarServer.Domain.Vehicles;
using RentCarServer.Domain.Vehicles.ValueObjects;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.Reservations;

public sealed record CreditCardInformation(
    string CardNumber,
    string CardOwner,
    string Expiry,
    string CCV);

[Permission("reservation:create")]
public sealed record ReservationCreateCommand(
    Guid CustomerId,
    Guid? PickUpLocationId,
    DateOnly PickUpDate,
    TimeOnly PickUpTime,
    DateOnly DeliveryDate,
    TimeOnly DeliveryTime,
    Guid VehicleId,
    decimal VehicleDailyPrice,
    Guid ProtectionPackageId,
    decimal ProtectionPackagePrice,
    IEnumerable<ReservationExtra> ReservationExtras,
    string Note,
    CreditCardInformation CreditCardInformation,
    decimal Total,
    int TotalDay
) : IRequest<Result<string>>;



public sealed class ReservationCreateCommandValidator : AbstractValidator<ReservationCreateCommand>
{
    public ReservationCreateCommandValidator()
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

        RuleFor(x => x.CreditCardInformation)
            .NotNull().WithMessage("Kart bilgileri boş olamaz.")
            .DependentRules(() =>
            {
                RuleFor(x => x.CreditCardInformation.CardNumber)
                    .NotEmpty().WithMessage("Kart numarası boş olamaz.");

                RuleFor(x => x.CreditCardInformation.CardOwner)
                    .NotEmpty().WithMessage("Kart sahibi adı boş olamaz.");

                RuleFor(x => x.CreditCardInformation.Expiry)
                    .NotEmpty().WithMessage("Son kullanma tarihi boş olamaz.");

                RuleFor(x => x.CreditCardInformation.CCV)
                    .NotEmpty().WithMessage("CVV boş olamaz.");
            });
    }
}

internal sealed class ReservationCreateCommandHandler(
    IReservationRepository reservationRepository,
    IBranchRepository branchRepository,
    ICustomerRepository customerRepository,
    IVehicleRepository vehicleRepository,
    IUnitOfWork unitOfWork,
    IClaimContext claimContext,
    IVehicleAvailabilityService vehicleAvailabilityService
    ) : IRequestHandler<ReservationCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ReservationCreateCommand request, CancellationToken cancellationToken)
    {
        var locationId = request.PickUpLocationId ?? claimContext.GetBranchId();

        #region Şube, Müşteri ve Araç Kontrolleri

        var isBranchExists = await branchRepository.AnyAsync(i => i.Id == locationId, cancellationToken);
        if (!isBranchExists)
            return Result<string>.Failure("Şube bilgisi bulunamadı.");

        var isCustomerExists = await customerRepository.AnyAsync(i => i.Id == request.CustomerId, cancellationToken);
        if (!isCustomerExists)
            return Result<string>.Failure("Müşteri bilgisi bulunamadı.");

        var isVehicleExists = await vehicleRepository.AnyAsync(i => i.Id == request.VehicleId, cancellationToken);
        if (!isVehicleExists)
            return Result<string>.Failure("Araç bilgisi bulunamadı.");

        #endregion

        var requestedPickUp = request.PickUpDate.ToDateTime(request.PickUpTime);
        var requestedDelivery = request.DeliveryDate.ToDateTime(request.DeliveryTime);

        var isAvailable = await vehicleAvailabilityService.IsAvailableAsync(
            new IdentityId(request.VehicleId), requestedPickUp, requestedDelivery, cancellationToken);

        if (!isAvailable)
            return Result<string>.Failure("Seçilen araç, belirtilen tarih ve saat aralığında müsait değil.");

        #region Ödeme İşlemi

        //ödeme işlemi yapılıp başarılı ise ona göre devam edilecek.


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

        IdentityId protectionPackageId = new(request.ProtectionPackageId);
        Price protectionPackagePrice = new(request.ProtectionPackagePrice);
        IEnumerable<ReservationExtra> reservationExtras = request.ReservationExtras.Select(s => new ReservationExtra(s.ExtraId, s.Price));
        Note note = new(request.Note);
        PaymentInformation paymentInformation = new(
            request.CreditCardInformation.CardNumber[^4..],
            request.CreditCardInformation.CardOwner);
        Status status = Status.Pending;
        Total total = new(request.Total);
        TotalDay totalDay = new(request.TotalDay);
        ReservationHistory history = new("Rezervasyon Oluşturuldu", "Online Olarak Rezervasyon Oluşturuldu", DateTimeOffset.Now);

        Form? prevPickUpForm = await reservationRepository
                    .Where(p => p.VehicleId == vehicleId)
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(s => s.PickUpForm)
                    .FirstOrDefaultAsync(cancellationToken);

        Form pickUpForm;
        Form deliveryForm;

        if (prevPickUpForm is null)
        {
            var kilometer = await vehicleRepository
                .Where(p => p.Id == request.VehicleId)
                .Select(s => s.Kilometer)
                .FirstAsync(cancellationToken);
            List<Supplies> supplies = new();
            List<Damage> damages = new();
            List<ImageUrl> imageUrls = new();
            Note formNote = new(string.Empty);
            pickUpForm = new(
                kilometer,
                supplies,
                imageUrls,
                damages,
                formNote);
        }
        else
        {
            pickUpForm = new(
                prevPickUpForm.Kilometer,
                prevPickUpForm.Supplies.ToList(),
                [],
                prevPickUpForm.Damages.ToList(),
                new(string.Empty));

            if (pickUpForm.Kilometer.Value == 0)
            {
                var kilometer = await vehicleRepository
                    .Where(p => p.Id == request.VehicleId)
                    .Select(s => s.Kilometer)
                    .FirstAsync(cancellationToken);
                pickUpForm.SetKilometer(kilometer);
            }
        }

        deliveryForm = new(
            pickUpForm.Kilometer,
            pickUpForm.Supplies.ToList(),
            pickUpForm.ImageUrls.ToList(),
            pickUpForm.Damages.ToList(),
            pickUpForm.Note);


        Reservation reservation = Reservation.Create(
            customerId,
            pickUpLocationId,
            pickUpDate,
            pickUpTime,
            deliveryDate,
            deliveryTime,
            vehicleId,
            vehicleDailyPrice,
            protectionPackageId,
            protectionPackagePrice,
            reservationExtras,
            note,
            paymentInformation,
            status,
            total,
            totalDay,
            history,
            pickUpForm,
            deliveryForm);

        #endregion

        ReservationHistory history2 = new("Ödeme Alındı", "Rezervasyon Ödemesi Başarıyla Alındı", DateTimeOffset.Now);
        reservation.SetHistory(history2);

        reservationRepository.Add(reservation);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Rezervasyon başarıyla oluşturuldu.";
    }
}