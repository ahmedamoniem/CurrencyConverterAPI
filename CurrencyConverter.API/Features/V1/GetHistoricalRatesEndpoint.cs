using CurrencyConverter.Application.DTOs;
using CurrencyConverter.Application.Interfaces;
using FastEndpoints;

namespace CurrencyConverter.Api.Features.V1;

public class GetHistoricalRatesEndpoint(ICurrencyService currencyService) 
    : Endpoint<HistoricalRatesRequestDto, PaginatedResult<ExchangeRateDto>>
{
    private readonly ICurrencyService _currencyService = currencyService;

    public override void Configure()
    {
        Verbs(Http.GET);
        Routes("/api/rates/historical");
        Roles("viewer", "admin");
        Version(1);
        Summary(s =>
        {
            s.Summary = "Get historical exchange rates";
            s.Description = "Returns historical exchange rates for a given base currency between the provided date range.";
            s.Response<ExchangeRateDto>(200, "Successfully retrieved historical exchange rates.");
            s.Response(400, "Bad request - invalid input or unsupported currency.");
            s.Response(401, "Unauthorized - missing or invalid JWT token.");
            s.Response(403, "Forbidden - user lacks required role.");
        });
    }

    public override async Task HandleAsync(HistoricalRatesRequestDto req, CancellationToken ct)
    {
        var result = await _currencyService.GetHistoricalRatesAsync(req);
        await SendOkAsync(result, ct);
    }
}
