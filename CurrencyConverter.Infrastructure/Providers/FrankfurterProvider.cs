using CurrencyConverter.Application.DTOs;
using CurrencyConverter.Application.Interfaces;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Throw;
using CurrencyConverter.Infrastructure.Providers.Models;

namespace CurrencyConverter.Infrastructure.Providers;

public class
   FrankfurterProvider(IHttpClientFactory httpClientFactory,
    ILogger<FrankfurterProvider> logger) : ICurrencyProvider
{

    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("FrankfurterClient");
    private readonly ILogger<FrankfurterProvider> _logger = logger;

    public async Task<ExchangeRateDto> GetRatesAsync(string baseCurrency)
    {
        baseCurrency.Throw("Base currency is required.").IfNullOrWhiteSpace(_ => _);
        var url = $"latest?base={baseCurrency.ToUpperInvariant()}";

        try
        {
            _logger.LogInformation("Fetching latest rates from Frankfurter API: {Url}", url);
            var response = await _httpClient.GetFromJsonAsync<FrankfurterApiResponse>(url);
            response.ThrowIfNull("Invalid response from currency provider");

            if (!DateTime.TryParse(response.Date, out var parsedDate))
            {
                _logger.LogWarning("Invalid date format received from Frankfurter: {Date}", response.Date);
                parsedDate = DateTime.UtcNow;
            }

            return new ExchangeRateDto(
                response.Base,
                parsedDate,
                response.Rates
            );
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error occurred while fetching latest rates.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while getting latest exchange rates.");
            throw;
        }
    }

    public async Task<ExchangeRateDto> GetHistoricalRatesAsync(string baseCurrency, DateTime startDate, DateTime endDate)
    {
        baseCurrency.Throw("Base currency is required.").IfNullOrWhiteSpace(_ => _);
        var url = $"{startDate:yyyy-MM-dd}..{endDate:yyyy-MM-dd}?base={baseCurrency.ToUpperInvariant()}";

        try
        {
            _logger.LogInformation("Fetching historical rates from Frankfurter API: {Url}", url);
            var response = await _httpClient.GetFromJsonAsync<FrankfurterHistoricalResponse>(url);
            response.ThrowIfNull("Invalid historical response from currency provider");

            var rates = response.Rates.ToDictionary(
                kvp => DateTime.TryParse(kvp.Key, out var date) ? date : DateTime.MinValue,
                kvp => kvp.Value
            );

            return new ExchangeRateDto(
                response.Base,
                DateTime.MinValue,
                [],
                rates
            );
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error occurred while fetching historical rates.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while getting historical exchange rates.");
            throw;
        }
    }
}
