using GenericRepository;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Abstractions;
using RentCarServer.Domain.Reservations;
using RentCarServer.Domain.Reservations.ValueObjects;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.CustomerPortal;

public sealed record CustomerCancelRentalCommand(Guid Id) : IRequest<Result<string>>;

internal sealed class CustomerCancelRentalCommandHandler(
    IReservationRepository reservationRepository,
    IUnitOfWork unitOfWork,
    IClaimContext claimContext) : IRequestHandler<CustomerCancelRentalCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CustomerCancelRentalCommand request, CancellationToken cancellationToken)
    {
        var customerId = new IdentityId(claimContext.GetUserId());

        var reservation = await reservationRepository
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.CustomerId == customerId, cancellationToken);

        if (reservation is null)
            return Result<string>.Failure("Rezervasyon bulunamadı.");

        if (reservation.Status.Value == Status.Canceled.Value)
            return Result<string>.Failure("Bu rezervasyon zaten iptal edilmiş.");

        if (reservation.Status.Value != Status.Pending.Value)
            return Result<string>.Failure("Sadece 'Bekliyor' durumundaki rezervasyonlar iptal edilebilir.");

        reservation.SetReservationStatus(Status.Canceled);
        reservationRepository.Update(reservation);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Rezervasyon başarıyla iptal edildi.";


    }
}