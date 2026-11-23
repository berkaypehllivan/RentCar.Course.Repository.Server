using RentCarServer.Application.CustomerPortal;
using RentCarServer.Application.Customers;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.WebAPI.Modules;

public static class CustomerPortalModule
{
    public static void MapCustomerPortal(this IEndpointRouteBuilder builder)
    {
        var app = builder.MapGroup("/customer-portal")
            .WithTags("CustomerPortal")
            .RequireAuthorization();

        app.MapGet("/profile",
            async (ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(new CustomerProfileQuery(), cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.BadRequest(res);
            })
            .Produces<Result<CustomerDto>>();

        app.MapPut("/profile",
            async (CustomerProfileUpdateCommand request, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(request, cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.BadRequest(res);
            })
            .Produces<Result<string>>();


        app.MapGet("/rentals",
    async (ISender sender, CancellationToken cancellationToken) =>
    {
        var res = await sender.Send(new CustomerRentalHistoryQuery(), cancellationToken);
        return res.IsSuccessful ? Results.Ok(res) : Results.BadRequest(res);
    })
    .Produces<Result<CustomerRentalHistoryResponse>>();

        app.MapGet("/rentals/{id:guid}",
    async (Guid id, ISender sender, CancellationToken cancellationToken) =>
    {
        var res = await sender.Send(new CustomerRentalDetailQuery(id), cancellationToken);
        return res.IsSuccessful ? Results.Ok(res) : Results.BadRequest(res);
    })
    .Produces<Result<CustomerRentalDetailDto>>();

        app.MapPut("/rentals/{id:guid}/cancel",
            async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(new CustomerCancelRentalCommand(id), cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.BadRequest(res);
            })
            .Produces<Result<string>>();

        app.MapPut("/change-password",
            async (CustomerChangePasswordCommand request, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(request, cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.BadRequest(res);
            })
            .RequireRateLimiting("login-fixed")
            .Produces<Result<string>>();
    }
}
