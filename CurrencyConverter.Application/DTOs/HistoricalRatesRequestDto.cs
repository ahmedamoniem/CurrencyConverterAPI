namespace CurrencyConverter.Application.DTOs;

public record HistoricalRatesRequestDto(
    string BaseCurrency,
    string TargetCurrency,
    DateTime StartDate,
    DateTime EndDate
);
