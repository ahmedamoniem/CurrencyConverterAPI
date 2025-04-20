
using CurrencyConverter.Api.Middlewares;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.TestCorrelator;
using System.Security.Claims;

namespace CurrencyConverter.Test.UnitTests;

public class RequestLoggingMiddlewareTests
{
    [Fact]
    public async Task LogsRequestDetails_WithClientIdAndIP()
    {
        // Arrange
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.TestCorrelator()
            .CreateLogger();

        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.0.5");
        context.Request.Method = "GET";
        context.Request.Path = "/api/rates";
        context.Response.StatusCode = 200;

        // Simulate user identity with client_id claim
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("client_id", "test-client-123")
        }, "mock"));

        var middleware = new RequestLoggingMiddleware(_ => Task.CompletedTask);

        // Act
        using (TestCorrelator.CreateContext())
        {
            await middleware.InvokeAsync(context);

            // Assert
            var log = TestCorrelator.GetLogEventsFromCurrentContext().SingleOrDefault();
            Assert.NotNull(log);
            Assert.Equal(LogEventLevel.Information, log.Level);
            Assert.Contains("test-client-123", log.RenderMessage());
            Assert.Contains("GET", log.RenderMessage());
            Assert.Contains("/api/rates", log.RenderMessage());
            Assert.Contains("192.168.0.5", log.RenderMessage());
            Assert.Contains("200", log.RenderMessage());
        }
    }

    [Fact]
    public async Task LogsRequestDetails_WithoutClientId()
    {
        // Arrange
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.TestCorrelator()
            .CreateLogger();

        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Loopback;
        context.Request.Method = "POST";
        context.Request.Path = "/api/convert";
        context.Response.StatusCode = 401;

        var middleware = new RequestLoggingMiddleware(_ => Task.CompletedTask);

        // Act
        using (TestCorrelator.CreateContext())
        {
            await middleware.InvokeAsync(context);

            // Assert
            var log = TestCorrelator.GetLogEventsFromCurrentContext().SingleOrDefault();
            Assert.NotNull(log);
            Assert.Contains("anonymous", log.RenderMessage());
            Assert.Contains("POST", log.RenderMessage());
            Assert.Contains("401", log.RenderMessage());
        }
    }
}
