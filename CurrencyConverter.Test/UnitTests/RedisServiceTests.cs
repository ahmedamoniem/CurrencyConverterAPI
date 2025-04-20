using CurrencyConverter.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Text.Json;
using Xunit;

namespace CurrencyConverter.Tests.UnitTests;

public class RedisServiceTests
{
    private readonly Mock<IDistributedCache> _mockCache = new();
    private readonly Mock<ILogger<RedisService>> _mockLogger = new();
    private readonly RedisService _service;

    public RedisServiceTests()
    {
        _service = new RedisService(_mockCache.Object, _mockLogger.Object);
    }

    private static byte[] SerializeToBytes<T>(T data) =>
        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));

    [Fact]
    public async Task GetAsync_ReturnsDeserializedObject_WhenCacheHit()
    {
        // Arrange
        var expected = new TestObject { Id = 1, Name = "Cached Item" };
        var bytes = SerializeToBytes(expected);

        _mockCache.Setup(c => c.GetAsync("test-key", default))
                  .ReturnsAsync(bytes);

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
        _mockCache.Setup(c => c.GetAsync("missing-key", default))
                  .ReturnsAsync((byte[]?)null);

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
        var expectedBytes = SerializeToBytes(data);

        _mockCache.Setup(c => c.SetAsync(
            "save-key",
            It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == Encoding.UTF8.GetString(expectedBytes)),
            It.IsAny<DistributedCacheEntryOptions>(),
            default)).Returns(Task.CompletedTask);

        // Act
        await _service.SetAsync("save-key", data, TimeSpan.FromMinutes(5));

        // Assert
        _mockCache.VerifyAll();
    }

    [Fact]
    public async Task RemoveAsync_DeletesFromCache()
    {
        // Arrange
        _mockCache.Setup(c => c.RemoveAsync("remove-key", default))
                  .Returns(Task.CompletedTask);

        // Act
        await _service.RemoveAsync("remove-key");

        // Assert
        _mockCache.Verify(c => c.RemoveAsync("remove-key", default), Times.Once);
    }

    private class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
