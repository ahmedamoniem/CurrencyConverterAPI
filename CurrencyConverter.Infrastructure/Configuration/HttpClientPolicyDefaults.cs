using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace CurrencyConverter.Infrastructure.Configuration;

public static class HttpClientPolicyDefaults
{
    public static IAsyncPolicy<HttpResponseMessage> RetryPolicy(ILogger logger) =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (outcome, timespan, attempt, context) =>
                {
                    logger.LogWarning("Retry {Attempt} after {Delay}s", attempt, timespan.TotalSeconds);
                });

    public static IAsyncPolicy<HttpResponseMessage> CircuitBreakerPolicy(ILogger logger) =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (result, delay) =>
                {
                    logger.LogError("Circuit breaker opened for {Delay}s", delay.TotalSeconds);
                },
                onReset: () =>
                {
                    logger.LogInformation("Circuit breaker reset");
                });

    public static IAsyncPolicy<HttpResponseMessage> TimeoutPolicy(ILogger logger) =>
        Policy.TimeoutAsync<HttpResponseMessage>(
            TimeSpan.FromSeconds(3),
            TimeoutStrategy.Pessimistic,
            onTimeoutAsync: (context, timespan, task, exception) =>
            {
                logger.LogWarning("Polly timeout triggered after {Timeout}s", timespan.TotalSeconds);
                return Task.CompletedTask;
            });
}
