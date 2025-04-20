using CurrencyConverter.Application.DTOs;

namespace CurrencyConverter.Application.Interfaces;

public interface ICurrencyProvider
{
    /// <summary>
    /// Use to Identify the provider for the factory
    /// </summary>
    string Name { get; }
    Task<ExchangeRateDto> GetRatesAsync(string baseCurrency);
    Task<ExchangeRateDto> GetHistoricalRatesAsync(string baseCurrency, DateTime startDate, DateTime endDate);
}
