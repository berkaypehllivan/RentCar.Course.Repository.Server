using Microsoft.AspNetCore.Mvc;
using RentCarServer.Application.Vehicles;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.WebAPI.Modules;

public static class VehicleModule
{
    public static void MapVehicle(this IEndpointRouteBuilder builder)
    {
        var app = builder.MapGroup("/vehicles")
            .RequireRateLimiting("fixed")
            .RequireAuthorization()
            .WithTags("Vehicles");

        app.MapPost(string.Empty,
            async ([FromForm] VehicleCreateCommand request, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(request, cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.InternalServerError(res);
            })
            .DisableAntiforgery()
            .Accepts<VehicleCreateCommand>("multipart/form-data")
            .Produces<Result<string>>();

        app.MapPut(string.Empty,
            async ([FromForm] VehicleUpdateCommand request, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(request, cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.InternalServerError(res);
            })
            .DisableAntiforgery()
            .Accepts<VehicleUpdateCommand>("multipart/form-data")
            .Produces<Result<string>>();

        app.MapDelete("{id}",
            async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(new VehicleDeleteCommand(id), cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.InternalServerError(res);
            })
            .Produces<Result<string>>();

        app.MapGet("{id}",
            async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(new VehicleGetQuery(id), cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.InternalServerError(res);
            })
            .Produces<Result<VehicleDto>>();

        app.MapGet(string.Empty,
            async (ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(new VehicleGetAllQuery(), cancellationToken);
                return Results.Ok(res);
            })
            .Produces<IQueryable<VehicleDto>>();
    }
}