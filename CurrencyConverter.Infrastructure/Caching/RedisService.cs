using CurrencyConverter.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace CurrencyConverter.Infrastructure.Caching;

public class RedisService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisService> _logger;

    public RedisService(IDistributedCache cache, ILogger<RedisService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var cachedBytes = await _cache.GetAsync(key);
            if (cachedBytes is null || cachedBytes.Length == 0)
                return default;

            var json = Encoding.UTF8.GetString(cachedBytes);
            return JsonSerializer.Deserialize<T>(json);
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
            var json = JsonSerializer.Serialize(data);
            var bytes = Encoding.UTF8.GetBytes(json);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow ?? TimeSpan.FromMinutes(10)
            };

            await _cache.SetAsync(key, bytes, options);
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
