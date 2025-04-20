using CurrencyConverter.Application.DTOs;

namespace CurrencyConverter.Application.Interfaces;

public interface ICurrencyService
{
    Task<ExchangeRateDto> GetLatestRatesAsync(string baseCurrency);

    Task<ConversionResponseDto> ConvertCurrencyAsync(ConversionRequestDto request);

    Task<PaginatedResult<ExchangeRateDto>> GetHistoricalRatesAsync(HistoricalRatesRequestDto request, int page = 1, int pageSize = 50);
}
