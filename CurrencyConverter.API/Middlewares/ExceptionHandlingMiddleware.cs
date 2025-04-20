using Microsoft.AspNetCore.Http;
using Serilog;
using System.Net;
using System.Text.Json;

namespace CurrencyConverter.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;
    private readonly Serilog.ILogger _logger = Log.ForContext<ExceptionHandlingMiddleware>();

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context); // call next middleware
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unhandled exception occurred during request");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex switch
            {
                NotSupportedException => (int)HttpStatusCode.BadRequest,
                ArgumentException => (int)HttpStatusCode.BadRequest,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var problem = new
            {
                status = context.Response.StatusCode,
                title = "An error occurred",
                detail = ex.Message
            };

            var json = JsonSerializer.Serialize(problem);
            await context.Response.WriteAsync(json);
        }
    }
}
