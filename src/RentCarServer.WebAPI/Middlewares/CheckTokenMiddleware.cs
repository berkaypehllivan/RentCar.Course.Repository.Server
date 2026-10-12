using Microsoft.AspNetCore.Authorization;
using RentCarServer.Domain.LoginTokens;
using System.Security.Claims;

namespace RentCarServer.WebAPI.Middlewares;

public sealed class CheckTokenMiddleware(
    ILoginTokenRepository loginTokenRepository) : IMiddleware
{
    public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
    {
        // Anonim endpoint'lerde token kontrolü yapma
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

        var isTokenAvailable = await loginTokenRepository.AnyAsync(p =>
            p.UserId == userId
            && p.Token.Value == token
            && p.IsActive.Value);
        if (!isTokenAvailable)
            throw new TokenException();

        await next(httpContext);
    }
}
public sealed class TokenException : Exception;