using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace RentCarServer.WebAPI;

public static class RateLimiterExtension
{
    public static RateLimiterOptions AddIpFixedWindowPolicy(
        this RateLimiterOptions options, string policyName, int permitLimit, TimeSpan window)
    {
        return options.AddPolicy(policyName, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(httpContext.Connection.RemoteIpAddress?.ToString() ?? "unkown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = window,
            QueueLimit = 0
        }));
    }
}
