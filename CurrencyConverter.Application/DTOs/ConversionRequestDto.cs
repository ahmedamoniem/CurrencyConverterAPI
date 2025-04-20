namespace CurrencyConverter.Application.DTOs;

public record ConversionRequestDto(
    string FromCurrency,
    string ToCurrency,
    decimal Amount
);
