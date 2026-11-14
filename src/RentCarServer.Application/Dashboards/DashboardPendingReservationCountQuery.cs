using Microsoft.EntityFrameworkCore;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Reservations;
using RentCarServer.Domain.Reservations.ValueObjects;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.Dashboards;

public sealed record DashboardPendingReservationCountQuery : IRequest<Result<int>>;

internal sealed class DashboardPendingReservationCountQueryHandler(
    IReservationRepository reservationRepository,
    IClaimContext claimContext) : IRequestHandler<DashboardPendingReservationCountQuery, Result<int>>
{
    public async Task<Result<int>> Handle(DashboardPendingReservationCountQuery request, CancellationToken cancellationToken)
    {
        var res = await reservationRepository
           .GetAll()
           .ForCurrentBranch(claimContext)
           .Where(p => p.Status.Value == Status.Pending.Value)
           .CountAsync(cancellationToken);

        return res;
    }
}