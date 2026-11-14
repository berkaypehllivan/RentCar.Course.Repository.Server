using Microsoft.EntityFrameworkCore;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Reservations;
using RentCarServer.Domain.Reservations.ValueObjects;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.Dashboards;

public sealed record DashboardMonthlyIncomeQuery : IRequest<Result<decimal>>;

internal sealed class DashboardMonthlyIncomeQueryHandler(IReservationRepository reservationRepository, IClaimContext claimContext) : IRequestHandler<DashboardMonthlyIncomeQuery, Result<decimal>>
{
    public async Task<Result<decimal>> Handle(DashboardMonthlyIncomeQuery request, CancellationToken cancellationToken)
    {
        var currentMonth = DateTime.Now.Month;
        var currentYear = DateTime.Now.Year;

        var result = await reservationRepository
.GetAll()
.ForCurrentBranch(claimContext)
.Where(p => p.CreatedAt.Month == currentMonth && p.CreatedAt.Year == currentYear)
.Where(p => p.Status.Value == Status.Completed.Value)
.SumAsync(p => p.Total.Value, cancellationToken);

        return result;
    }
}