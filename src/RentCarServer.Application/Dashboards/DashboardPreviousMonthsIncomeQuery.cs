using Microsoft.EntityFrameworkCore;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Reservations;
using RentCarServer.Domain.Reservations.ValueObjects;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.Dashboards;

public sealed record DashboardPreviousMonthsIncomeQuery : IRequest<Result<List<DashboardMonthlyIncomeDto>>>;

public sealed record DashboardMonthlyIncomeDto(int Year, int Month, decimal Amount);

internal sealed class DashboardPreviousMonthsIncomeQueryHandler(
    IReservationRepository reservationRepository, IClaimContext claimContext) : IRequestHandler<DashboardPreviousMonthsIncomeQuery, Result<List<DashboardMonthlyIncomeDto>>>
{
    private const int MonthCount = 3;

    public async Task<Result<List<DashboardMonthlyIncomeDto>>> Handle(DashboardPreviousMonthsIncomeQuery request, CancellationToken cancellationToken)
    {
        // En yakın ay önce gelsin (Kasım için: Ekim, Eylül, Ağustos). Ocak/Şubat/Mart'ta önceki yıla taşar.
        var startOfCurrentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var months = Enumerable.Range(1, MonthCount)
            .Select(i => startOfCurrentMonth.AddMonths(-i))
            .ToList();
        var years = months.Select(m => m.Year).Distinct().ToList();

        var incomes = await reservationRepository
            .GetAll()
            .ForCurrentBranch(claimContext)
            .Where(p => years.Contains(p.CreatedAt.Year))
            .Where(p => p.Status.Value == Status.Completed.Value)
            .GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Amount = g.Sum(p => p.Total.Value) })
            .ToListAsync(cancellationToken);

        // Geliri olmayan aylar da 0 olarak dönsün
        var res = months
            .Select(d => new DashboardMonthlyIncomeDto(
                d.Year,
                d.Month,
                incomes.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month)?.Amount ?? 0))
            .ToList();

        return res;
    }
}