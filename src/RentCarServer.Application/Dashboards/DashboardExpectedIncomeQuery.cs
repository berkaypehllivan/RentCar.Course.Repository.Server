using Microsoft.EntityFrameworkCore;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Reservations;
using RentCarServer.Domain.Reservations.ValueObjects;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.Dashboards;

public sealed record DashboardExpectedIncomeQuery : IRequest<Result<decimal>>;

internal sealed class DashboardExpectedIncomeQueryHandler(
    IReservationRepository reservationRepository,
    IClaimContext claimContext) : IRequestHandler<DashboardExpectedIncomeQuery, Result<decimal>>
{
    public async Task<Result<decimal>> Handle(DashboardExpectedIncomeQuery request, CancellationToken cancellationToken)
    {
        var result = await reservationRepository
            .GetAll()
            .ForCurrentBranch(claimContext)
            .Where(p => p.Status.Value != Status.Completed.Value && p.Status.Value != Status.Canceled.Value)
            .SumAsync(p => p.Total.Value, cancellationToken);

        return result;
    }
}