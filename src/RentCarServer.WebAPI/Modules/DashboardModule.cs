using RentCarServer.Application.Dashboards;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.WebAPI.Modules;

public static class DashboardModule
{
    public static void MapDashboard(this IEndpointRouteBuilder builder)
    {
        var app = builder
                .MapGroup("/dashboard")
                .RequireRateLimiting("fixed")
                .RequireAuthorization()
                .WithTags("Dashboard");

        // ---------- Rezervasyonlar ----------
        app.MapGet("active-reservation-count", async (ISender sender, CancellationToken cancellationToken) =>
        {
            var res = await sender.Send(new DashboardActiveReservationCountQuery(), cancellationToken);
            return res.IsSuccessful ? Results.Ok(res) : Results.InternalServerError(res);
        })
            .Produces<Result<int>>();

        app.MapGet("pending-reservation-count", async (ISender sender, CancellationToken cancellationToken) =>
        {
            var res = await sender.Send(new DashboardPendingReservationCountQuery(), cancellationToken);
            return res.IsSuccessful ? Results.Ok(res) : Results.InternalServerError(res);
        }).Produces<Result<int>>();

        app.MapGet("completed-reservation-count", async (ISender sender, CancellationToken cancellationToken) =>
        {
            var res = await sender.Send(new DashboardCompletedReservationCountQuery(), cancellationToken);
            return res.IsSuccessful ? Results.Ok(res) : Results.InternalServerError(res);
        }).Produces<Result<int>>();

        app.MapGet("canceled-reservation-count", async (ISender sender, CancellationToken cancellationToken) =>
        {
            var res = await sender.Send(new DashboardCanceledReservationCountQuery(), cancellationToken);
            return res.IsSuccessful ? Results.Ok(res) : Results.InternalServerError(res);
        }).Produces<Result<int>>();

        // ---------- Araçlar ----------
        app.MapGet("total-vehicle-count", async (ISender sender, CancellationToken cancellationToken) =>
        {
            var res = await sender.Send(new DashboardTotalVehicleCountQuery(), cancellationToken);
            return res.IsSuccessful ? Results.Ok(res) : Results.InternalServerError(res);
        }).Produces<Result<int>>();

        app.MapGet("branch-vehicle-counts", async (ISender sender, CancellationToken cancellationToken) =>
        {
            var res = await sender.Send(new DashboardBranchVehicleCountsQuery(), cancellationToken);
            return res.IsSuccessful ? Results.Ok(res) : Results.InternalServerError(res);
        }).Produces<Result<List<DashboardBranchVehicleCountDto>>>();

        // ---------- Gelir ----------
        app.MapGet("monthly-income", async (ISender sender, CancellationToken cancellationToken) =>
        {
            var res = await sender.Send(new DashboardMonthlyIncomeQuery(), cancellationToken);
            return res.IsSuccessful ? Results.Ok(res) : Results.InternalServerError(res);
        }).Produces<Result<decimal>>();

        app.MapGet("previous-months-income", async (ISender sender, CancellationToken cancellationToken) =>
        {
            var res = await sender.Send(new DashboardPreviousMonthsIncomeQuery(), cancellationToken);
            return res.IsSuccessful ? Results.Ok(res) : Results.InternalServerError(res);
        }).Produces<Result<List<DashboardMonthlyIncomeDto>>>();

        app.MapGet("yearly-income", async (ISender sender, CancellationToken cancellationToken) =>
        {
            var res = await sender.Send(new DashboardYearlyIncomeQuery(), cancellationToken);
            return res.IsSuccessful ? Results.Ok(res) : Results.InternalServerError(res);
        }).Produces<Result<decimal>>();

        app.MapGet("expected-income", async (ISender sender, CancellationToken cancellationToken) =>
        {
            var res = await sender.Send(new DashboardExpectedIncomeQuery(), cancellationToken);
            return res.IsSuccessful ? Results.Ok(res) : Results.InternalServerError(res);
        }).Produces<Result<decimal>>();

        // ---------- Grafikler ----------
        app.MapGet("revenue-chart", async (ISender sender, CancellationToken cancellationToken, string period = "7d") =>
            {
                var res = await sender.Send(new DashboardRevenueChartQuery(period), cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.InternalServerError(res);
            }).Produces<Result<List<DashboardChartPointDto>>>();

        // ---------- Müşteriler ----------
        app.MapGet("total-customer-count", async (ISender sender, CancellationToken cancellationToken) =>
        {
            var res = await sender.Send(new DashboardTotalCustomerCountQuery(), cancellationToken);
            return res.IsSuccessful ? Results.Ok(res) : Results.InternalServerError(res);
        }).Produces<Result<int>>();
    }
}