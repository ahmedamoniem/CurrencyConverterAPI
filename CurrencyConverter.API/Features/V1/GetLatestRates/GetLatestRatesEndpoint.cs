using CurrencyConverter.Application.DTOs;
using CurrencyConverter.Application.Interfaces;
using FastEndpoints;

namespace CurrencyConverter.API.Features.V1.GetLatestRates;

public class GetLatestRatesEndpoint(ICurrencyService currencyService) 
    : Endpoint<GetLatestRatesRequest, ExchangeRateDto>
{
    private readonly ICurrencyService _currencyService = currencyService;

    public override void Configure()
    {
        Verbs(Http.GET);
        Routes("/api/rates/latest");
        Roles("viewer", "admin");
        Version(1);
        Summary(s =>
        {
            s.Summary = "Get the latest exchange rates";
            s.Description = "Returns the most recent exchange rates for the specified base currency.";
            s.Response<ExchangeRateDto>(200, "Successfully retrieved exchange rates.");
            s.Response(401, "Unauthorized - missing or invalid JWT token.");
            s.Response(403, "Forbidden - user lacks required role.");
        });
    }

    public override async Task HandleAsync(GetLatestRatesRequest req, CancellationToken ct)
    {
        var result = await _currencyService.GetLatestRatesAsync(req.BaseCurrency);
        await SendOkAsync(result, ct);
    }
}
