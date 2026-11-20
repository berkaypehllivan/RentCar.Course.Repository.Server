using FluentValidation;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Abstractions;
using RentCarServer.Domain.Branchs;
using RentCarServer.Domain.Customers;
using RentCarServer.Domain.Extras;
using RentCarServer.Domain.ProtectionPackages;
using RentCarServer.Domain.Reservations;
using RentCarServer.Domain.Reservations.Forms;
using RentCarServer.Domain.Reservations.ValueObjects;
using RentCarServer.Domain.Shared;
using RentCarServer.Domain.Vehicles;
using RentCarServer.Domain.Vehicles.ValueObjects;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.Booking;

public sealed record BookingPaymentDto(
    string CardNumber,
    string CardOwner,
    string Expiry,
    string CCV);
public sealed record BookingCreateCommand(
    Guid PickUpLocationId,
    DateOnly PickUpDate,
    TimeOnly PickUpTime,
    DateOnly DeliveryDate,
    TimeOnly DeliveryTime,
    Guid VehicleId,
    Guid ProtectionPackageId,
    List<Guid> ExtraIds,
    string? Note,
    BookingPaymentDto PaymentInformation) : IRequest<Result<string>>;

public sealed class BookingCreateCommandValidator : AbstractValidator<BookingCreateCommand>
{
    public BookingCreateCommandValidator()
    {
        RuleFor(x => x.PickUpLocationId)
            .NotEmpty().WithMessage("Teslim alış şubesi boş olamaz.");
        RuleFor(x => x.VehicleId)
            .NotEmpty().WithMessage("Araç bilgisi boş olamaz.");
        RuleFor(x => x.ProtectionPackageId)
            .NotEmpty().WithMessage("Güvence paketi seçilmelidir.");
        RuleFor(x => x.PickUpDate).NotEmpty().GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today)).WithMessage("Alış tarihi bugünden eski olamaz.");
        RuleFor(x => x.DeliveryDate).NotEmpty().GreaterThanOrEqualTo(x => x.PickUpDate).WithMessage("Teslim tarihi, alış tarihinden önce olamaz.");
        RuleFor(x => x).Must(x => x.DeliveryDate.ToDateTime(x.DeliveryTime) > x.PickUpDate.ToDateTime(x.PickUpTime))
            .WithMessage("Teslim tarihi ve saati, alış tarih ve saatinden önce olamaz.");
        RuleFor(x => x.PaymentInformation).NotNull().WithMessage("Ödeme bilgileri doldurulmalıdır.")
            .DependentRules(() =>
            {
                RuleFor(x => x.PaymentInformation.CardNumber)
                .NotEmpty().MinimumLength(16).WithMessage("Geçerli bir kart numarası giriniz.");
                RuleFor(x => x.PaymentInformation.CardOwner).NotEmpty().WithMessage("Kart sahibi adı boş olamaz.");
                RuleFor(x => x.PaymentInformation.Expiry).NotEmpty().WithMessage("Son kullanma tarihi boş olamaz.");
                RuleFor(x => x.PaymentInformation.CCV).NotEmpty().WithMessage("CVV kodu boş olamaz.");
            });
    }

    internal sealed class BookingCreateCommandHandler(
        IReservationRepository reservationRepository,
        IBranchRepository branchRepository,
        ICustomerRepository customerRepository,
        IVehicleRepository vehicleRepository,
        IProtectionPackageRepository protectionPackageRepository,
        IExtraRepository extraRepository,
        IUnitOfWork unitOfWork,
        IClaimContext claimContext,
        IVehicleAvailabilityService vehicleAvailabilityService) : IRequestHandler<BookingCreateCommand, Result<string>>
    {
        public async Task<Result<string>> Handle(BookingCreateCommand request, CancellationToken cancellationToken)
        {
            #region Kullanıcı, Şube, Araç, Güvence Paketi ve Ekstra Kontrolleri

            var customerId = claimContext.GetUserId();
            var isCustomerExists = await customerRepository
                .AnyAsync(c => c.Id == customerId, cancellationToken);

            if (!isCustomerExists)
                return Result<string>.Failure("Müşteri bilgisi bulunamadı.");

            var isBranchExists = await branchRepository
                .AnyAsync(b => b.Id == request.PickUpLocationId, cancellationToken);
            if (!isBranchExists)
                return Result<string>.Failure("Seçilen şube bulunamadı.");

            var vehicle = await vehicleRepository
                .FirstOrDefaultAsync(v => v.Id == request.VehicleId && v.IsActive, cancellationToken);
            if (vehicle is null)
                return Result<string>.Failure("Seçilen araç bulunamadı.");

            var protectionPackage = await protectionPackageRepository
                .FirstOrDefaultAsync(p => p.Id == request.ProtectionPackageId && p.IsActive, cancellationToken);
            if (protectionPackage is null)
                return Result<string>.Failure("Seçilen güvence paketi bulunamadı.");

            var distinctExtraIds = request.ExtraIds?.Distinct().ToList() ?? [];
            var extras = distinctExtraIds.Count > 0 ? await extraRepository
                .Where(e => distinctExtraIds.Contains(e.Id) && e.IsActive)
                .ToListAsync(cancellationToken) : [];
            if (extras.Count != distinctExtraIds.Count)
                return Result<string>.Failure("Seçilen ek hizmetler bulunamadı.");

            #endregion

            #region Müsaitlik Kontrolü

            var requestedPickUp = request.PickUpDate.ToDateTime(request.PickUpTime);
            var requestedDelivery = request.DeliveryDate.ToDateTime(request.DeliveryTime);

            var isAvaliable = await vehicleAvailabilityService.IsAvailableAsync(new IdentityId(request.VehicleId), requestedPickUp, requestedDelivery, cancellationToken);
            if (!isAvaliable)
                return Result<string>.Failure("Seçilen araç, belirtilen tarih ve saat aralığında artık müsait değil.");

            #endregion

            #region Tutar Hesaplama

            var calculatedTotalDay = CalculateTotalDay(requestedPickUp, requestedDelivery);
            var vehicleTotal = vehicle.DailyPrice.Value * calculatedTotalDay;
            var protectionTotal = protectionPackage.Price.Value * calculatedTotalDay;
            var extrasTotal = extras.Sum(e => e.Price.Value);
            var grandTotal = vehicleTotal + protectionTotal + extrasTotal;

            #endregion

            #region Ödeme İşlemi

            //Ödeme işlemi entegrasyonu burada sağlanacak.
            var cleanCardNumber = request.PaymentInformation.CardNumber.Replace(" ", string.Empty);
            #endregion

            #region Rezervasyon Nesnesinin Oluşturulması

            var vehicleIdentityId = new IdentityId(request.VehicleId);
            var (pickUpForm, deliveryForm) = await CreateInitialFormsAsync(vehicleIdentityId, vehicle.Kilometer, cancellationToken);

            var paymentInformation = new PaymentInformation(
                       cleanCardNumber[^4..],
                       request.PaymentInformation.CardOwner.Trim());

            var initialHistory = new ReservationHistory(
                "Rezervasyon Oluşturuldu",
                "Online Olarak Rezervasyon Oluşturuldu",
                DateTime.Now);

            var reservationExtras = extras
                .Select(e => new ReservationExtra(e.Id, e.Price.Value))
                .ToList();

            var reservation = Reservation.Create(
                new IdentityId(customerId),
                new IdentityId(request.PickUpLocationId),
                new PickUpDate(request.PickUpDate),
                new PickUpTime(request.PickUpTime),
                new DeliveryDate(request.DeliveryDate),
                new DeliveryTime(request.DeliveryTime),
                vehicleIdentityId,
                new Price(vehicle.DailyPrice.Value),
                new IdentityId(protectionPackage.Id),
                new Price(protectionPackage.Price.Value),
                reservationExtras,
                new Note(request.Note ?? string.Empty),
                paymentInformation,
                Status.Pending,
                new Total(grandTotal),
                new TotalDay(calculatedTotalDay),
                initialHistory,
                pickUpForm,
                deliveryForm);

            var paymentHistory = new ReservationHistory(
                "Ödeme Alındı",
                "Online Rezervasyon Ödemesi Başarıyla Alındı", DateTimeOffset.Now);

            reservation.SetHistory(paymentHistory);

            reservationRepository.Add(reservation);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Succeed(reservation.ReservationNumber.Value);

            #endregion
        }
        private int CalculateTotalDay(DateTime pickUpDateTime, DateTime deliveryDateTime)
        {
            var totalDays = (deliveryDateTime - pickUpDateTime).Days;
            var sameDayExtraAllowed = deliveryDateTime.TimeOfDay <= pickUpDateTime.TimeOfDay.Add(TimeSpan.FromHours(2));

            if (totalDays == 0 || (totalDays == 1 && sameDayExtraAllowed))
                return 1;

            return sameDayExtraAllowed ? totalDays : totalDays + 1;
        }

        private async Task<(Form PickUpForm, Form DeliveryForm)> CreateInitialFormsAsync(
            IdentityId vehicleId,
            Kilometer currentVehicleKilometer,
            CancellationToken cancellationToken)
        {
            var prevPickUpForm = await reservationRepository
                .Where(p => p.VehicleId == vehicleId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(s => s.PickUpForm)
                .FirstOrDefaultAsync(cancellationToken);

            Form pickUpForm;
            if (prevPickUpForm is null)
                pickUpForm = new Form(currentVehicleKilometer, [], [], [], new Note(string.Empty));
            else
            {
                var kilometer = prevPickUpForm.Kilometer.Value == 0
                    ? currentVehicleKilometer
                    : prevPickUpForm.Kilometer;

                pickUpForm = new Form(
                    kilometer,
                    prevPickUpForm.Supplies.ToList(),
                    [],
                    prevPickUpForm.Damages.ToList(),
                    new Note(string.Empty));
            }

            var deliveryForm = new Form(
                pickUpForm.Kilometer,
                pickUpForm.Supplies.ToList(),
                pickUpForm.ImageUrls.ToList(),
                pickUpForm.Damages.ToList(),
                pickUpForm.Note);

            return (pickUpForm, deliveryForm);

        }
    }
}