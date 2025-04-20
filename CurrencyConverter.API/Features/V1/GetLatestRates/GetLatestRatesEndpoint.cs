using CurrencyConverter.Application.DTOs;
using CurrencyConverter.Application.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace CurrencyConverter.API.Features.V1.GetLatestRates;


[Authorize(Roles = "viewer,admin")]
public class GetLatestRatesEndpoint(ICurrencyService currencyService) : Endpoint<GetLatestRatesRequest, ExchangeRateDto>
{
    private readonly ICurrencyService _currencyService = currencyService;

    public override void Configure()
    {
        Verbs(Http.GET);
        Routes("/api/rates/latest");
        Version(1);
    }

    public override async Task HandleAsync(GetLatestRatesRequest req, CancellationToken ct)
    {
        var result = await _currencyService.GetLatestRatesAsync(req.BaseCurrency);
        await SendOkAsync(result, ct);
    }
}
