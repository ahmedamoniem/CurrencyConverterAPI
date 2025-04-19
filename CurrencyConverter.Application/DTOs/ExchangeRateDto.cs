namespace CurrencyConverter.Application.DTOs;
public record ExchangeRateDto(
    string Base,
    DateTime Date,
    Dictionary<string, decimal> Rates,
    Dictionary<DateTime, Dictionary<string, decimal>>? HistoricalRates = null
);