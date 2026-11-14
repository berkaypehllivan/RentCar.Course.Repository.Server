using Microsoft.EntityFrameworkCore;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Reservations;
using RentCarServer.Domain.Reservations.ValueObjects;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.Dashboards;

public sealed record DashboardCanceledReservationCountQuery : IRequest<Result<int>>;

internal sealed class DashboardCanceledReservationCountQueryHandler(
    IReservationRepository reservationRepository,
    IClaimContext claimContext) : IRequestHandler<DashboardCanceledReservationCountQuery, Result<int>>
{
    public async Task<Result<int>> Handle(DashboardCanceledReservationCountQuery request, CancellationToken cancellationToken)
    {
        // İptal edilen rezervasyonlar (tüm şubeler)
        var res = await reservationRepository
           .GetAll()
           .ForCurrentBranch(claimContext)
           .Where(p => p.Status.Value == Status.Canceled.Value)
           .CountAsync(cancellationToken);

        return res;
    }
}