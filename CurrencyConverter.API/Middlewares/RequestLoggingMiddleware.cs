using Serilog;
using System.Diagnostics;
using System.Security.Claims;

namespace CurrencyConverter.Api.Middlewares;

public class RequestLoggingMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;
    private readonly Serilog.ILogger _logger = Log.ForContext<RequestLoggingMiddleware>();

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        string? clientId = context.User?.FindFirst("client_id")?.Value
                        ?? context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? "anonymous";

        var method = context.Request.Method;
        var path = context.Request.Path;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;

            _logger.Information(
                "Request from IP {IP}, ClientId {ClientId}: {Method} {Path} => {StatusCode} in {Elapsed:0.0000} ms",
                ipAddress,
                clientId,
                method,
                path,
                statusCode,
                stopwatch.Elapsed.TotalMilliseconds
            );
        }
    }
}
