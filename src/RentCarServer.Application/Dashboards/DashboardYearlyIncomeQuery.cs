using Microsoft.EntityFrameworkCore;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Reservations;
using RentCarServer.Domain.Reservations.ValueObjects;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.Dashboards;

public sealed record DashboardYearlyIncomeQuery : IRequest<Result<decimal>>;

internal sealed class DashboardYearlyIncomeQueryHandler(
    IReservationRepository reservationRepository, IClaimContext claimContext) : IRequestHandler<DashboardYearlyIncomeQuery, Result<decimal>>
{
    public async Task<Result<decimal>> Handle(DashboardYearlyIncomeQuery request, CancellationToken cancellationToken)
    {
        var currentYear = DateTime.Now.Year;

        var result = await reservationRepository
            .GetAll()
            .ForCurrentBranch(claimContext)
            .Where(p => p.CreatedAt.Year == currentYear)
            .Where(p => p.Status.Value == Status.Completed.Value)
            .SumAsync(p => p.Total.Value, cancellationToken);

        return result;
    }
}