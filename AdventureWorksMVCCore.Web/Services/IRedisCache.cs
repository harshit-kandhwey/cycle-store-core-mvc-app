using System;
using System.Threading.Tasks;

namespace AdventureWorksMVCCore.Web.Services
{
    /// <summary>
    /// Interface for Redis cache operations to replace static collections.
    /// </summary>
    public interface IRedisCache
    {
        /// <summary>
        /// Gets a cached value by key, or computes and caches it if not present.
        /// </summary>
        Task<string> GetOrAddAsync(string key, Func<string, string> valueFactory, TimeSpan? expiration = null);

        /// <summary>
        /// Gets a cached value by key.
        /// </summary>
        Task<string> GetAsync(string key);

        /// <summary>
        /// Sets a cached value by key.
        /// </summary>
        Task SetAsync(string key, string value, TimeSpan? expiration = null);

        /// <summary>
        /// Removes a cached value by key.
        /// </summary>
        Task RemoveAsync(string key);
    }
}
