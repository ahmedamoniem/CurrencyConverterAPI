using CurrencyConverter.Application.DTOs;
using CurrencyConverter.Application.Factories;
using CurrencyConverter.Application.Interfaces;
using CurrencyConverter.Domain.Enums;

namespace CurrencyConverter.Application.Services;

public class CurrencyService(CurrencyProviderFactory providerFactory) : ICurrencyService
{
    private readonly CurrencyProviderFactory _providerFactory = providerFactory;

    public async Task<ExchangeRateDto> GetLatestRatesAsync(string baseCurrency)
    {
        ValidateCurrency(baseCurrency);

        var provider = _providerFactory.Create("frankfurter");
        return await provider.GetRatesAsync(baseCurrency);
    }

    public async Task<ConversionResponseDto> ConvertCurrencyAsync(ConversionRequestDto request)
    {
        ValidateCurrency(request.FromCurrency);
        ValidateCurrency(request.ToCurrency);

        var provider = _providerFactory.Create("frankfurter");
        var rates = await provider.GetRatesAsync(request.FromCurrency);

        if (!rates.Rates.TryGetValue(request.ToCurrency, out var rate))
        {
            throw new Exception($"Exchange rate from {request.FromCurrency} to {request.ToCurrency} not found.");
        }

        var converted = request.Amount * rate;

        return new ConversionResponseDto(
            request.FromCurrency,
            request.ToCurrency,
            request.Amount,
            converted,
            rate,
            rates.Date
        );
    }

    public async Task<PaginatedResult<ExchangeRateDto>> GetHistoricalRatesAsync(
        HistoricalRatesRequestDto request, int page = 1, int pageSize = 50)
    {
        ValidateCurrency(request.BaseCurrency);
        ValidateCurrency(request.TargetCurrency);

        var provider = _providerFactory.Create("frankfurter");
        var history = await provider.GetHistoricalRatesAsync(request.BaseCurrency, request.StartDate, request.EndDate);

        var all = history.HistoricalRates!
            .Where(x => x.Value.ContainsKey(request.TargetCurrency))
            .OrderByDescending(x => x.Key)
            .ToList();

        var paged = all
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(entry => new ExchangeRateDto(
                request.BaseCurrency,
                entry.Key,
                new Dictionary<string, decimal> { [request.TargetCurrency] = entry.Value[request.TargetCurrency] }
            ));

        return new PaginatedResult<ExchangeRateDto>(paged, page, pageSize, all.Count);
    }

    private void ValidateCurrency(string currencyCode)
    {
        if (Enum.TryParse<UnsupportedCurrencies>(currencyCode, true, out _))
            throw new NotSupportedException($"Currency '{currencyCode}' is not supported.");
    }
}
