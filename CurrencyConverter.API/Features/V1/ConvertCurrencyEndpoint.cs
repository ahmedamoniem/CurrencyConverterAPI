using CurrencyConverter.Application.DTOs;
using CurrencyConverter.Application.Interfaces;
using FastEndpoints;

namespace CurrencyConverter.Api.Features.V1;

public class ConvertCurrencyEndpoint(ICurrencyService currencyService) 
    : Endpoint<ConversionRequestDto, ConversionResponseDto>
{
    private readonly ICurrencyService _currencyService = currencyService;

    public override void Configure()
    {
        Verbs(Http.POST);
        Routes("/api/rates/convert");
        Roles("viewer", "admin");
        Version(1);
        Summary(s =>
        {
            s.Summary = "Converts an amount from one currency to another.";
            s.Description = "Provides conversion result based on real-time exchange rates.";
            s.Response<ConversionResponseDto>(200, "Successful currency conversion result.");
            s.Response(400, "Bad request - invalid currency or unsupported conversion.");
            s.Response(401, "Unauthorized - missing or invalid JWT token.");
            s.Response(403, "Forbidden - user lacks required role.");
        });
    }

    public override async Task HandleAsync(ConversionRequestDto req, CancellationToken ct)
    {
        var result = await _currencyService.ConvertCurrencyAsync(req);
        await SendOkAsync(result, ct);
    }
}
