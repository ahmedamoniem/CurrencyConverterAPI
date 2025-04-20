namespace CurrencyConverter.Application.DTOs;

public record ConversionResponseDto(
    string FromCurrency,
    string ToCurrency,
    decimal OriginalAmount,
    decimal ConvertedAmount,
    decimal Rate,
    DateTime Date
);
