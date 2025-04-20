using FastEndpoints;

namespace CurrencyConverter.API.Features.V1.GetLatestRates;

public class GetLatestRatesRequest
{
    [FromQuery]
    public string BaseCurrency { get; set; } = "USD";
}
