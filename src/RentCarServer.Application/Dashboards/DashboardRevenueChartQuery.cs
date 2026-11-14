using Microsoft.EntityFrameworkCore;
using RentCarServer.Application.Services;
using RentCarServer.Domain.Reservations;
using RentCarServer.Domain.Reservations.ValueObjects;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.Application.Dashboards;

public sealed record DashboardRevenueChartQuery(string Period) : IRequest<Result<List<DashboardChartPointDto>>>;

public sealed record DashboardChartPointDto(DateOnly Date, decimal Total);

internal sealed class DashboardRevenueChartQueryHandler(IReservationRepository reservationRepository, IClaimContext claimContext) : IRequestHandler<DashboardRevenueChartQuery, Result<List<DashboardChartPointDto>>>
{
    public async Task<Result<List<DashboardChartPointDto>>> Handle(DashboardRevenueChartQuery request, CancellationToken cancellationToken)
    {
        if (request.Period is not ("7d" or "30d" or "3m" or "12m"))
            return Result<List<DashboardChartPointDto>>.Failure("Geçersiz periyot!");

        var today = DateOnly.FromDateTime(DateTime.Now);
        var thisMonth = new DateOnly(today.Year, today.Month, 1);

        // Veritabanında günlük toplamları al
        var (startDate, isMonthly) = request.Period switch
        {
            "7d" => (today.AddDays(-6), false),
            "30d" => (today.AddDays(-29), false),
            "3m" => (thisMonth.AddMonths(-2), true),
            _ => (thisMonth.AddMonths(-11), true)
        };

        // Bellekte tam aralığa daralt
        var dailyTotals = await reservationRepository
            .GetAll()
            .ForCurrentBranch(claimContext)
            .Where(p => p.Status.Value == Status.Completed.Value)
            .Where(p => p.CreatedAt.Year == startDate.Year)
            .GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month, p.CreatedAt.Day })
            .Select(g => new { g.Key.Year, g.Key.Month, g.Key.Day, Total = g.Sum(p => p.Total.Value) })
            .ToListAsync(cancellationToken);

        var totals = dailyTotals
            .Select(x => new { Date = new DateOnly(x.Year, x.Month, x.Day), x.Total })
            .Where(x => x.Date >= startDate && x.Date <= today)
            .ToList();

        // Boş günleri/ayları 0 ile doldurarak grafik için eksiksiz seri üret
        List<DashboardChartPointDto> points;
        if (isMonthly)
        {
            var monthCount = (thisMonth.Year - startDate.Year) * 12 + thisMonth.Month - startDate.Month + 1;
            points = Enumerable.Range(0, monthCount)
                .Select(i => startDate.AddMonths(i))
                .Select(m => new DashboardChartPointDto(
                    m,
                    totals.Where(x => x.Date.Year == m.Year && x.Date.Month == m.Month).Sum(x => x.Total)))
                .ToList();
        }
        else
        {
            var dayCount = today.DayNumber - startDate.DayNumber + 1;
            points = Enumerable.Range(0, dayCount)
                .Select(i => startDate.AddDays(i))
                .Select(d => new DashboardChartPointDto(
                    d,
                    totals.Where(x => x.Date == d).Sum(x => x.Total)))
                .ToList();
        }

        return Result<List<DashboardChartPointDto>>.Succeed(points);
    }
}