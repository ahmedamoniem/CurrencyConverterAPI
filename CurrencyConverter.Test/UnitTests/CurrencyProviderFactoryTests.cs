using CurrencyConverter.Application.Factories;
using CurrencyConverter.Application.Interfaces;
using CurrencyConverter.Application.DTOs;

namespace CurrencyConverter.Test.UnitTests;

public class CurrencyProviderFactoryTests
{
    private class FakeFrankfurterProvider : ICurrencyProvider
    {
        public string Name => "frankfurter";

        public Task<ExchangeRateDto> GetRatesAsync(string baseCurrency) => throw new NotImplementedException();
        public Task<ExchangeRateDto> GetHistoricalRatesAsync(string baseCurrency, DateTime startDate, DateTime endDate) => throw new NotImplementedException();
    }

    [Fact]
    public void Create_ReturnsProvider_WhenProviderExists()
    {
        // Arrange
        var provider = new FakeFrankfurterProvider();
        var factory = new CurrencyProviderFactory([provider]);

        // Act
        var resolved = factory.Create("frankfurter");

        // Assert
        Assert.NotNull(resolved);
        Assert.IsType<FakeFrankfurterProvider>(resolved);
    }

    [Fact]
    public void Create_Throws_WhenProviderDoesNotExist()
    {
        // Arrange
        var factory = new CurrencyProviderFactory(Array.Empty<ICurrencyProvider>());

        // Act & Assert
        var ex = Assert.Throws<NotSupportedException>(() => factory.Create("nonexistent"));
        Assert.Contains("not supported", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
