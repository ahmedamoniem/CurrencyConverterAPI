namespace CurrencyConverter.Application.DTOs;

public record HistoricalRatesRequestDto(
    string BaseCurrency,
    string TargetCurrency,
    DateTime StartDate,
    DateTime EndDate,
    // TODO: those could be fichied from the query string
    int Page = 1, 
    int PageSize = 50
);
