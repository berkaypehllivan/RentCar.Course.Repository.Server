using RentCarServer.Application.Booking;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.WebAPI.Modules;

public static class BookingModule
{
    public static void MapBooking(this IEndpointRouteBuilder builder)
    {
        var app = builder.MapGroup("/booking").WithTags("Booking").AllowAnonymous();

        app.MapPost("/available-vehicles",
            async (AvaiableVehiclesQuery request, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(request, cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.InternalServerError(res);
            })
            .Produces<Result<List<AvaiableVehicleDto>>>();

        app.MapGet("/branches",
            async (ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(new BranchListQuery(), cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.InternalServerError(res);
            })
            .Produces<Result<List<BranchListDto>>>();

        app.MapGet("/protection-packages",
            async (ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(new ProtectionPackageListQuery(), cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.InternalServerError(res);
            })
            .Produces<Result<List<ProtectionPackageListDto>>>();

        app.MapGet("/extras",
            async (ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(new ExtraListQuery(), cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.InternalServerError(res);
            })
            .Produces<Result<List<ExtraListDto>>>();

        app.MapPost("/",
            async (BookingCreateCommand request, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(request, cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.BadRequest(res);
            })
            .RequireAuthorization()
            .Produces<Result<string>>();
    }
}
