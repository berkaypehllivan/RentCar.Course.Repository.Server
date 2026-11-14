using Microsoft.EntityFrameworkCore;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Reservations;
using RentCarServer.Domain.Reservations.ValueObjects;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.Dashboards;

public sealed record DashboardCompletedReservationCountQuery : IRequest<Result<int>>;

internal sealed class DashboardCompletedReservationCountQueryHandler(
    IReservationRepository reservationRepository,
    IClaimContext claimContext) : IRequestHandler<DashboardCompletedReservationCountQuery, Result<int>>
{
    public async Task<Result<int>> Handle(DashboardCompletedReservationCountQuery request, CancellationToken cancellationToken)
    {
        // Tamamlanan rezervasyonlar (tüm şubeler)
        var res = await reservationRepository
           .GetAll()
           .ForCurrentBranch(claimContext)
           .Where(p => p.Status.Value == Status.Completed.Value)
           .CountAsync(cancellationToken);


        return res;
    }
}