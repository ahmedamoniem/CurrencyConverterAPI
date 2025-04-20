using CurrencyConverter.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace CurrencyConverter.Test.UnitTests;

public class HttpClientPoliciesTests
{
    private readonly ILogger _logger;

    public HttpClientPoliciesTests()
    {
        var loggerFactory = new Mock<ILoggerFactory>();
        _logger = Mock.Of<ILogger>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(_logger);
    }

    [Fact]
    public async Task RetryPolicy_ShouldRetryThreeTimes()
    {
        var policy = HttpClientPolicyDefaults.RetryPolicy(_logger);
        int attempt = 0;

        await policy.ExecuteAsync(() =>
        {
            attempt++;
            if (attempt < 4)
                throw new HttpRequestException("Simulated failure");

            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        });

        Assert.Equal(4, attempt);
    }

    [Fact]
    public async Task CircuitBreakerPolicy_ShouldOpenAfterFailures()
    {
        var policy = HttpClientPolicyDefaults.CircuitBreakerPolicy(_logger);

        // Fail 5 times to trigger breaker
        for (int i = 0; i < 5; i++)
        {
            await Assert.ThrowsAsync<HttpRequestException>(() =>
                policy.ExecuteAsync(() => throw new HttpRequestException("Simulated error")));
        }

        // Breaker should now be open
        await Assert.ThrowsAsync<BrokenCircuitException>(() =>
            policy.ExecuteAsync(() => Task.FromResult(new HttpResponseMessage())));
    }

    [Fact]
    public async Task TimeoutPolicy_ShouldThrowTimeoutException()
    {
        var policy = HttpClientPolicyDefaults.TimeoutPolicy(_logger);

        await Assert.ThrowsAsync<TimeoutRejectedException>(() =>
            policy.ExecuteAsync(async ct =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5), ct); // exceeds timeout
                return new HttpResponseMessage();
            }, CancellationToken.None));
    }
}
