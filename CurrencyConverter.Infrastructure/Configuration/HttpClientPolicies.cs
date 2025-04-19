using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

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
                .AddPolicyHandler((sp, _) => GetTimeoutPolicy(sp.GetRequiredService<ILoggerFactory>().CreateLogger(clientName)))
                .AddPolicyHandler((sp, _) => GetRetryPolicy(sp.GetRequiredService<ILoggerFactory>().CreateLogger(clientName)))
                .AddPolicyHandler((sp, _) => GetCircuitBreakerPolicy(sp.GetRequiredService<ILoggerFactory>().CreateLogger(clientName)));
        }
    }

    private static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(ILogger logger)
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(3, TimeoutStrategy.Pessimistic, onTimeoutAsync: (context, timespan, task, exception) =>
        {
            logger.LogWarning("Polly timeout triggered after {Timeout}s", timespan.TotalSeconds);
            return Task.CompletedTask;
        });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ILogger logger)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    logger.LogWarning("Retrying HTTP request. Attempt {RetryAttempt}. Delay: {Delay}s", retryAttempt, timespan.TotalSeconds);
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(ILogger logger)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (result, breakDelay) =>
                {
                    logger.LogError("Circuit breaker opened. Breaking for {BreakDelay}s", breakDelay.TotalSeconds);
                },
                onReset: () =>
                {
                    logger.LogInformation("Circuit breaker reset.");
                });
    }
}
