using CurrencyConverter.Application.DTOs;

namespace CurrencyConverter.Application.Interfaces;

public interface ICurrencyProvider
{
    Task<ExchangeRateDto> GetRatesAsync(string baseCurrency);
    Task<ExchangeRateDto> GetHistoricalRatesAsync(string baseCurrency, DateTime startDate, DateTime endDate);
}
