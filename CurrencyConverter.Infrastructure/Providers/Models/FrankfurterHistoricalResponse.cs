namespace CurrencyConverter.Infrastructure.Providers.Models;

public record FrankfurterHistoricalResponse(
    string Base,
    Dictionary<string, Dictionary<string, decimal>> Rates
);