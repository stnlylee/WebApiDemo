using Microsoft.Extensions.Caching.Distributed;
using System.Threading.Tasks;

namespace WebApiDemo.Domain.Cache
{
    public interface IDistributedCacheProvider
    {
        Task<T> GetFromCache<T>(string key) where T : class;
        
        Task SetCache<T>(string key, T value,
            DistributedCacheEntryOptions options) where T : class;
    }
}
