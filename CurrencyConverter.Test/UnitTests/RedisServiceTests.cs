using CurrencyConverter.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace CurrencyConverter.Test.UnitTests;

public class RedisServiceTests
{
    private readonly Mock<IDistributedCache> _mockCache = new();
    private readonly Mock<ILogger<RedisService>> _mockLogger = new();
    private readonly RedisService _service;

    public RedisServiceTests()
    {
        _service = new RedisService(_mockCache.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAsync_ReturnsDeserializedObject_WhenCacheHit()
    {
        // Arrange
        var expected = new TestObject { Id = 1, Name = "Cached Item" };
        var json = JsonSerializer.Serialize(expected);
        _mockCache.Setup(c => c.GetStringAsync("test-key", default))
                  .ReturnsAsync(json);

        // Act
        var result = await _service.GetAsync<TestObject>("test-key");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected.Id, result!.Id);
        Assert.Equal(expected.Name, result.Name);
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenCacheMiss()
    {
        // Arrange
        _mockCache.Setup(c => c.GetStringAsync("missing-key", default))
                  .ReturnsAsync((string?)null);

        // Act
        var result = await _service.GetAsync<TestObject>("missing-key");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_SerializesAndStoresData()
    {
        // Arrange
        var data = new TestObject { Id = 2, Name = "Store Me" };
        string expectedJson = JsonSerializer.Serialize(data);

        _mockCache.Setup(c => c.SetStringAsync(
            "save-key",
            It.Is<string>(json => json == expectedJson),
            It.IsAny<DistributedCacheEntryOptions>(),
            default)).Returns(Task.CompletedTask);

        // Act
        await _service.SetAsync("save-key", data, TimeSpan.FromMinutes(5));

        // Assert
        _mockCache.VerifyAll();
    }

    private class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
