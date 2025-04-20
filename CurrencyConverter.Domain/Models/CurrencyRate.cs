namespace CurrencyConverter.Domain.Models;

public class CurrencyRate
{
    public string BaseCurrency { get; set; } = default!;
    public string TargetCurrency { get; set; } = default!;
    public decimal Rate { get; set; }
    public DateTime Date { get; set; }
}
