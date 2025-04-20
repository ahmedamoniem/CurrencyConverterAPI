using CurrencyConverter.Application.DTOs;
using CurrencyConverter.Application.Interfaces;
using CurrencyConverter.Infrastructure.Providers;
using CurrencyConverter.Infrastructure.Providers.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace CurrencyConverter.Test.UnitTests;

public class FrankfurterProviderTests
{
    private readonly Mock<ILogger<FrankfurterProvider>> _mockLogger = new();
    private readonly Mock<ICacheService> _mockCache = new();

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
        var mockResponse = new FrankfurterApiResponse("EUR", "2024-04-01", new() { { "USD", 1.1m } });
        var httpClient = CreateMockHttpClient(mockResponse);
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(f => f.CreateClient("FrankfurterClient")).Returns(httpClient);

        _mockCache.Setup(c => c.GetAsync<ExchangeRateDto>(It.IsAny<string>())).ReturnsAsync((ExchangeRateDto?)null);

        var provider = new FrankfurterProvider(httpClientFactory.Object, _mockLogger.Object, _mockCache.Object);

        var result = await provider.GetRatesAsync("EUR");

        Assert.Equal("EUR", result.Base);
        Assert.Contains("USD", result.Rates.Keys);
        Assert.Equal(1.1m, result.Rates["USD"]);
    }

    [Fact]
    public async Task GetHistoricalRatesAsync_ReturnsExchangeRateDtoWithDates()
    {
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

        _mockCache.Setup(c => c.GetAsync<ExchangeRateDto>(It.IsAny<string>())).ReturnsAsync((ExchangeRateDto?)null);

        var provider = new FrankfurterProvider(httpClientFactory.Object, _mockLogger.Object, _mockCache.Object);

        var result = await provider.GetHistoricalRatesAsync("EUR", new DateTime(2024, 03, 31), new DateTime(2024, 04, 01));

        Assert.Equal("EUR", result.Base);
        Assert.NotNull(result.HistoricalRates);
        Assert.Equal(2, result.HistoricalRates!.Count);
    }

    [Fact]
    public async Task GetRatesAsync_ThrowsException_WhenResponseIsNull()
    {
        HttpClient client = CreateMockHttpClient<FrankfurterApiResponse>(null!);
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("FrankfurterClient")).Returns(client);

        var provider = new FrankfurterProvider(factory.Object, _mockLogger.Object, _mockCache.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() => provider.GetRatesAsync("EUR"));
    }

    [Fact]
    public async Task GetRatesAsync_ThrowsException_OnHttpError()
    {
        var handler = new Mock<HttpMessageHandler>();

        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Internal Server Error")
            });

        var client = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://api.frankfurter.app/")
        };

        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("FrankfurterClient")).Returns(client);

        var provider = new FrankfurterProvider(factory.Object, _mockLogger.Object, _mockCache.Object);

        await Assert.ThrowsAsync<HttpRequestException>(() => provider.GetRatesAsync("EUR"));
    }

    [Fact]
    public async Task GetRatesAsync_ThrowsException_WhenBaseCurrencyIsEmpty()
    {
        var factory = new Mock<IHttpClientFactory>();
        var client = CreateMockHttpClient(new FrankfurterApiResponse("EUR", "2024-04-01", []));
        factory.Setup(f => f.CreateClient("FrankfurterClient")).Returns(client);

        var provider = new FrankfurterProvider(factory.Object, _mockLogger.Object, _mockCache.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => provider.GetRatesAsync(""));
    }
}
