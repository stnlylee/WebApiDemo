using Microsoft.Extensions.Caching.Distributed;
using Moq;
using System.Text;
using System.Threading.Tasks;
using WebApiDemo.Domain.Cache;
using Xunit;

namespace WebApiDemo.UnitTests.DistributedCacheTests
{
    public class DistributedCacheProviderTests
    {
        private readonly Mock<IDistributedCache> _mockDistributedCache;
        private readonly DistributedCacheProvider _distributedCacheProvider;

        private class CacheTestClass
        {
            public string Field1 { get; set; }
        }

        public DistributedCacheProviderTests()
        {
            _mockDistributedCache = new Mock<IDistributedCache>();
            _distributedCacheProvider = new DistributedCacheProvider(_mockDistributedCache.Object);
        }

        [Fact]
        public async Task DistributedCacheProvider_GetFromCache_Should_Get_Value()
        {
            // Arrange
            var bytes = Encoding.Default.GetBytes("{\"Field1\": \"Value1\"}");
            _mockDistributedCache.Setup(x => x.GetAsync(It.IsAny<string>(), default)).ReturnsAsync(bytes);

            // Act
            var result = await _distributedCacheProvider.GetFromCache<CacheTestClass>("key");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Value1", result.Field1);
        }

        [Fact]
        public async Task DistributedCacheProvider_SetCache_Should_Set_Value()
        {
            // Arrange
            CacheTestClass cacheTestClass = new CacheTestClass
            {
                Field1 = "Value1"
            };
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions();

            // Act
            await _distributedCacheProvider.SetCache<CacheTestClass>("key", cacheTestClass, options);

            // Assert
            _mockDistributedCache.Verify(x => x.SetAsync(It.IsAny<string>(), 
                It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), default), Times.Once);
        }
    }
}
