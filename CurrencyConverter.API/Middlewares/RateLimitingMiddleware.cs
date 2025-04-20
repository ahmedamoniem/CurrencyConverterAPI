using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using System.Net;
using System.Text;

namespace CurrencyConverter.Api.Middlewares;

public class RateLimitingMiddleware(RequestDelegate next, IDistributedCache cache)
{
    private readonly RequestDelegate _next = next;
    private readonly IDistributedCache _cache = cache;
    private readonly Serilog.ILogger _logger = Log.ForContext<RateLimitingMiddleware>();

    private const int Limit = 100; // requests
    private static readonly TimeSpan Period = TimeSpan.FromMinutes(1);

    public async Task InvokeAsync(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var key = $"ratelimit:{ip}";

        var cached = await _cache.GetStringAsync(key);
        var count = string.IsNullOrEmpty(cached) ? 0 : int.Parse(cached);

        if (count >= Limit)
        {
            _logger.Warning("Rate limit exceeded for {IP}", ip);
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
            return;
        }

        count++;
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = Period
        };

        await _cache.SetStringAsync(key, count.ToString(), options);

        await _next(context);
    }
}
