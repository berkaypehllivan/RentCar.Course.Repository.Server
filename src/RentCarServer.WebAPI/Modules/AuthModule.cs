using RentCarServer.Application.Auth;
using RentCarServer.Application.Auth.Customers;
using TS.MediatR;
using TS.Result;

namespace RentCarServer.WebAPI.Modules;

public static class AuthModule
{
    public static void MapAuth(this IEndpointRouteBuilder builder)
    {
        var app = builder.MapGroup("/auth").WithTags("Auth").AllowAnonymous();

        app.MapPost("/login",
            async (UserLoginCommand request, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(request, cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.InternalServerError(res);
            })
            .Produces<Result<UserLoginCommandResponse>>()
            .RequireRateLimiting("login-fixed");

        app.MapPost("/login-with-tfa",
    async (UserLoginWithTFACommand request, ISender sender, CancellationToken cancellationToken) =>
    {
        var res = await sender.Send(request, cancellationToken);
        return res.IsSuccessful ? Results.Ok(res) : Results.InternalServerError(res);
    })
    .Produces<Result<UserLoginCommandResponse>>()
    .RequireRateLimiting("login-fixed");

        //Customer login side
        app.MapPost("customer-login",
            async (CustomerLoginCommand request, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(request, cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.InternalServerError(res);
            })
            .Produces<Result<CustomerLoginCommandResponse>>()
            .RequireRateLimiting("login-fixed");

        app.MapPost("/customer-register",
    async (CustomerRegisterCommand request, ISender sender, CancellationToken cancellationToken) =>
    {
        var res = await sender.Send(request, cancellationToken);
        return res.IsSuccessful ? Results.Ok(res) : Results.InternalServerError(res);
    })
    .Produces<Result<string>>()
    .RequireRateLimiting("register-fixed");

        app.MapPost("/forgot-password/{email}",
            async (string email, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(new ForgotPasswordCommand(email), cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.InternalServerError(res);
            })
            .Produces<Result<string>>()
            .RequireRateLimiting("forgot-password-fixed");

        app.MapPost("/reset-password",
            async (ResetPasswordCommand request, ISender sender, CancellationToken cancellationToken) =>
            {
                var res = await sender.Send(request, cancellationToken);
                return res.IsSuccessful ? Results.Ok(res) : Results.InternalServerError(res);
            })
            .Produces<Result<string>>()
            .RequireRateLimiting("reset-password-fixed");

        app.MapGet("/check-forgot-password-code/{forgotPasswordCode}",
           async (Guid forgotPasswordCode, ISender sender, CancellationToken cancellationToken) =>
           {
               var res = await sender.Send(new CheckForgotPasswordCodeCommand(forgotPasswordCode), cancellationToken);
               return res.IsSuccessful ? Results.Ok(res) : Results.InternalServerError(res);
           })
           .Produces<Result<string>>()
           .RequireRateLimiting("check-forgot-password-code-fixed");
    }
}
