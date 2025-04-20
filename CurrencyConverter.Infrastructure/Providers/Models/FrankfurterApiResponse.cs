namespace CurrencyConverter.Infrastructure.Providers.Models;

public record FrankfurterApiResponse(
 string Base,
 string Date,
 Dictionary<string, decimal> Rates
);
