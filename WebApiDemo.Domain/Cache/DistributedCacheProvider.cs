using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebApiDemo.Domain.Cache
{
    public class DistributedCacheProvider : IDistributedCacheProvider
    {
        private readonly IDistributedCache _cache;

        public DistributedCacheProvider(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<T> GetFromCache<T>(string key) where T : class
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            var cachedBytes = await _cache.GetAsync(key);

            if (cachedBytes != null && cachedBytes.Length > 0)
            {
                var value = Encoding.Default.GetString(cachedBytes);

                return string.IsNullOrEmpty(value) ? 
                    null : JsonSerializer.Deserialize<T>(value);
            }

            return null;
        }

        public async Task SetCache<T>(string key, T value, DistributedCacheEntryOptions options) where T : class
        {
            if (!string.IsNullOrEmpty(key) && value != null)
            {
                var serializedValue = JsonSerializer.Serialize(value);
                await _cache.SetAsync(key, Encoding.Default.GetBytes(serializedValue), options);
            }
        }
    }
}
