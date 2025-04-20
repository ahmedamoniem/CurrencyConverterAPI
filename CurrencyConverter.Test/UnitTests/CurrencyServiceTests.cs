using CurrencyConverter.Application.DTOs;
using CurrencyConverter.Application.Factories;
using CurrencyConverter.Application.Interfaces;
using CurrencyConverter.Application.Services;
using Moq;

namespace CurrencyConverter.Test.UnitTests;

public class CurrencyServiceTests
{
    private readonly Mock<ICurrencyProvider> _mockProvider;
    private readonly CurrencyService _service;

    public CurrencyServiceTests()
    {
        _mockProvider = new Mock<ICurrencyProvider>();
        _mockProvider.Setup(p => p.Name).Returns("frankfurter");

        var factory = new CurrencyProviderFactory(new[] { _mockProvider.Object });

        _service = new CurrencyService(factory);
    }

    [Fact]
    public async Task GetLatestRatesAsync_ReturnsRates()
    {
        var dto = new ExchangeRateDto("USD", DateTime.UtcNow, new Dictionary<string, decimal> { ["EUR"] = 1.1m });
        _mockProvider.Setup(p => p.GetRatesAsync("USD")).ReturnsAsync(dto);

        var result = await _service.GetLatestRatesAsync("USD");

        Assert.Equal("USD", result.Base);
        Assert.Contains("EUR", result.Rates.Keys);
    }

    [Fact]
    public async Task ConvertCurrencyAsync_ReturnsConvertedAmount()
    {
        var dto = new ExchangeRateDto("USD", DateTime.UtcNow, new Dictionary<string, decimal> { ["EUR"] = 1.1m });
        _mockProvider.Setup(p => p.GetRatesAsync("USD")).ReturnsAsync(dto);

        var request = new ConversionRequestDto("USD", "EUR", 100);
        var result = await _service.ConvertCurrencyAsync(request);

        Assert.Equal(110m, result.ConvertedAmount);
    }

    [Fact]
    public async Task GetHistoricalRatesAsync_ReturnsPagedData()
    {
        var today = DateTime.UtcNow.Date;
        var dto = new ExchangeRateDto("USD", today, [], new()
        {
            [today.AddDays(-2)] = new() { ["EUR"] = 1.1m },
            [today.AddDays(-1)] = new() { ["EUR"] = 1.2m },
            [today] = new() { ["EUR"] = 1.3m }
        });

        _mockProvider.Setup(p =>
            p.GetHistoricalRatesAsync("USD", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(dto);

        var request = new HistoricalRatesRequestDto("USD", "EUR", today.AddDays(-5), today);
        var result = await _service.GetHistoricalRatesAsync(request);

        Assert.Equal(2, result.Items.Count());
        Assert.Equal(3, result.TotalCount);
        Assert.True(result.HasNextPage);
    }

    [Theory]
    [InlineData("TRY")]
    [InlineData("PLN")]
    [InlineData("THB")]
    [InlineData("MXN")]
    public async Task GetLatestRatesAsync_Throws_WhenUnsupportedCurrency(string currency)
    {
        await Assert.ThrowsAsync<NotSupportedException>(() =>
            _service.GetLatestRatesAsync(currency));
    }

    [Fact]
    public async Task ConvertCurrencyAsync_Throws_WhenRateNotFound()
    {
        var dto = new ExchangeRateDto("USD", DateTime.UtcNow, new Dictionary<string, decimal>()); // EUR missing
        _mockProvider.Setup(p => p.GetRatesAsync("USD")).ReturnsAsync(dto);

        var request = new ConversionRequestDto("USD", "EUR", 100);
        await Assert.ThrowsAsync<Exception>(() => _service.ConvertCurrencyAsync(request));
    }
}
