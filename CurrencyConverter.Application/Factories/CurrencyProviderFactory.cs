using CurrencyConverter.Application.Interfaces;

namespace CurrencyConverter.Application.Factories;

public class CurrencyProviderFactory(IEnumerable<ICurrencyProvider> providers)
{
    private readonly IEnumerable<ICurrencyProvider> _providers = providers;

    public ICurrencyProvider Create(string providerName)
    {
        var provider = _providers.FirstOrDefault(p =>
            string.Equals(p.Name, providerName, StringComparison.OrdinalIgnoreCase));

        return provider ?? throw new NotSupportedException($"Currency provider '{providerName}' is not supported.");
    }
}
