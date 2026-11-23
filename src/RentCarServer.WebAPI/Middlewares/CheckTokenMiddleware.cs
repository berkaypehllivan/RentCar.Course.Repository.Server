using Microsoft.AspNetCore.Authorization;
using RentCarServer.Domain.LoginTokens;
using System.Security.Claims;

namespace RentCarServer.WebAPI.Middlewares;

public sealed class CheckTokenMiddleware(
    IUserLoginTokenRepository loginTokenRepository,
    ICustomerLoginTokenRepository customerLoginTokenRepository) : IMiddleware
{
    public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        var endpoint = httpContext.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null)
        {
            await next(httpContext);
            return;
        }

        var authHeader = httpContext.Request.Headers.Authorization.ToString();
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            await next(httpContext);
            return;
        }

        var token = authHeader["Bearer ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            await next(httpContext);
            return;
        }

        var userId = httpContext.User.Claims
            .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            throw new TokenException();

        var userType = httpContext.User.Claims
            .FirstOrDefault(x => x.Type == "user_type")?.Value;

        var isTokenAvailable = userType == "customer"
            ? await customerLoginTokenRepository.AnyAsync(p =>
            p.CustomerId == userId
            && p.Token.Value == token
            && p.IsActive.Value)
            : await loginTokenRepository.AnyAsync(p =>
            p.UserId == userId
            && p.Token.Value == token
            && p.IsActive.Value);

        if (!isTokenAvailable)
            throw new TokenException();

        await next(httpContext);
    }
}
public sealed class TokenException : Exception;