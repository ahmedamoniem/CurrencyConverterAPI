using CurrencyConverter.Api.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using System.Text;

namespace CurrencyConverter.Test.UnitTests;

public class RateLimitingMiddlewareTests
{
    private readonly Mock<IDistributedCache> _mockCache = new();
    private readonly DefaultHttpContext _httpContext = new();
    private readonly RequestDelegate _next = static (ctx) => Task.CompletedTask;

    [Fact]
    public async Task AllowsRequest_WhenUnderLimit()
    {
        // Arrange
        _httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        _mockCache.Setup(c => c.GetAsync(It.IsAny<string>(), default))
            .ReturnsAsync(Encoding.UTF8.GetBytes("5"));

        var middleware = new RateLimitingMiddleware(_next, _mockCache.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.NotEqual(429, _httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task BlocksRequest_WhenLimitExceeded()
    {
        // Arrange
        _httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.0.1");
        _mockCache.Setup(c => c.GetAsync(It.IsAny<string>(), default))
          .ReturnsAsync(Encoding.UTF8.GetBytes("100"));


        var middleware = new RateLimitingMiddleware(_next, _mockCache.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Equal(429, _httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task InitializesRequestCount_WhenKeyNotExists()
    {
        // Arrange
        _httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("10.0.0.1");
        _mockCache.Setup(c => c.GetAsync(It.IsAny<string>(), default))
            .ReturnsAsync((byte[]?)null);


        var middleware = new RateLimitingMiddleware(_next, _mockCache.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _mockCache.Verify(c => c.SetAsync(
               It.IsAny<string>(),
               It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == "1"),
               It.IsAny<DistributedCacheEntryOptions>(),
               default
           ), Times.Once);

    }
}
