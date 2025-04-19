using CurrencyConverter.Infrastructure.Providers;
using CurrencyConverter.Infrastructure.Providers.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace CurrencyConverter.Test.UnitTests;

public class FrankfurterProviderTests
{
    private readonly Mock<ILogger<FrankfurterProvider>> _mockLogger = new();

    private static HttpClient CreateMockHttpClient<T>(T responseData, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handler = new Mock<HttpMessageHandler>();

        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = JsonContent.Create(responseData, options: new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                })
            });

        return new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://api.frankfurter.app/")
        };
    }

    [Fact]
    public async Task GetRatesAsync_ReturnsExchangeRateDto()
    {
        // Arrange
        var mockResponse = new FrankfurterApiResponse("EUR", "2024-04-01", new() { { "USD", 1.1m } });
        var httpClient = CreateMockHttpClient(mockResponse);

        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(f => f.CreateClient("FrankfurterClient")).Returns(httpClient);

        var provider = new FrankfurterProvider(httpClientFactory.Object, _mockLogger.Object);

        // Act
        var result = await provider.GetRatesAsync("EUR");

        // Assert
        Assert.Equal("EUR", result.Base);
        Assert.Contains("USD", result.Rates.Keys);
        Assert.Equal(1.1m, result.Rates["USD"]);
    }

    [Fact]
    public async Task GetHistoricalRatesAsync_ReturnsExchangeRateDtoWithDates()
    {
        // Arrange
        var mockResponse = new FrankfurterHistoricalResponse(
            "EUR",
            new()
            {
                { "2024-03-31", new() { { "USD", 1.2m } } },
                { "2024-04-01", new() { { "USD", 1.3m } } }
            });

        var httpClient = CreateMockHttpClient(mockResponse);
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(f => f.CreateClient("FrankfurterClient")).Returns(httpClient);

        var provider = new FrankfurterProvider(httpClientFactory.Object, _mockLogger.Object);

        // Act
        var result = await provider.GetHistoricalRatesAsync("EUR", new DateTime(2024, 03, 31), new DateTime(2024, 04, 01));

        // Assert
        Assert.Equal("EUR", result.Base);
        Assert.NotNull(result.HistoricalRates);
        Assert.Equal(2, result.HistoricalRates!.Count);
    }
}
