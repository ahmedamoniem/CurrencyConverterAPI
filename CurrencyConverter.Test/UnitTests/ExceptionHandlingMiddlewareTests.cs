using CurrencyConverter.Api.Middleware;
using Microsoft.AspNetCore.Http;

namespace CurrencyConverter.Test.UnitTests;

public class ExceptionHandlingMiddlewareTests
{
    private static DefaultHttpContext CreateContext() =>
        new DefaultHttpContext
        {
            Response = { Body = new MemoryStream() }
        };

    private static async Task<string> InvokeMiddlewareWithException(Exception ex)
    {
        var context = CreateContext();

        var middleware = new ExceptionHandlingMiddleware(_ => throw ex);
        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }

    [Fact]
    public async Task Handles_NotSupportedException_With400()
    {
        var result = await InvokeMiddlewareWithException(new NotSupportedException("Not supported"));

        Assert.Contains("Not supported", result);
        Assert.Contains("400", result);
    }

    [Fact]
    public async Task Handles_ArgumentException_With400()
    {
        var result = await InvokeMiddlewareWithException(new ArgumentException("Bad input"));

        Assert.Contains("Bad input", result);
        Assert.Contains("400", result);
    }

    [Fact]
    public async Task Handles_KeyNotFoundException_With404()
    {
        var result = await InvokeMiddlewareWithException(new KeyNotFoundException("Not found"));

        Assert.Contains("Not found", result);
        Assert.Contains("404", result);
    }

    [Fact]
    public async Task Handles_GenericException_With500()
    {
        var result = await InvokeMiddlewareWithException(new Exception("Server error"));

        Assert.Contains("Server error", result);
        Assert.Contains("500", result);
    }
}
