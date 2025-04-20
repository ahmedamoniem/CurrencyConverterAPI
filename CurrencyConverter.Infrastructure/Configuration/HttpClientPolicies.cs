using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CurrencyConverter.Infrastructure.Configuration;

public static class HttpClientPolicies
{

    public static void AddHttpClientPolicies(this IServiceCollection services, IConfiguration configuration)
    {
        var clientConfig = configuration.GetSection("ApiClients").Get<Dictionary<string, string>>();
        ArgumentNullException.ThrowIfNull(clientConfig);

        services.Configure<ApiClientOptions>(configuration.GetSection("ApiClients"));

        var sp = services.BuildServiceProvider();
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("PollyLogger");

        foreach (var (clientName, baseUrl) in clientConfig)
        {
            services.AddHttpClient(clientName, client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = Timeout.InfiniteTimeSpan;
            })
            

             .AddPolicyHandler((sp, _) =>
                  {
                      var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger(clientName);
                      return HttpClientPolicyDefaults.TimeoutPolicy(logger);
                  })
             .AddPolicyHandler((sp, _) =>
                {
                    var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger(clientName);
                    return HttpClientPolicyDefaults.RetryPolicy(logger);
                })
             .AddPolicyHandler((sp, _) =>
                {
                    var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger(clientName);
                    return HttpClientPolicyDefaults.CircuitBreakerPolicy(logger);
                });
        }
    }
}
