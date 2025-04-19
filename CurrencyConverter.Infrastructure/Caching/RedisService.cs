using CurrencyConverter.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CurrencyConverter.Infrastructure.Caching;

public class RedisService(IDistributedCache cache, ILogger<RedisService> logger)
    : ICacheService
{
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger<RedisService> _logger = logger;

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var cachedData = await _cache.GetStringAsync(key);
            if (string.IsNullOrWhiteSpace(cachedData))
                return default;

            return JsonSerializer.Deserialize<T>(cachedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading from Redis cache with key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T data, TimeSpan? absoluteExpirationRelativeToNow = null)
    {
        try
        {
            var serializedData = JsonSerializer.Serialize(data);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow ?? TimeSpan.FromMinutes(10)
            };

            await _cache.SetStringAsync(key, serializedData, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing to Redis cache with key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing key from Redis cache: {Key}", key);
        }
    }
}
