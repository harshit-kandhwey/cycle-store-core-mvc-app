using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace AdventureWorksMVCCore.Web.Services
{
    /// <summary>
    /// Redis cache implementation using Amazon ElastiCache for Redis.
    /// Replaces static collections with distributed cache for multi-instance deployments.
    /// </summary>
    public class RedisCache : IRedisCache
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public RedisCache(IConnectionMultiplexer redis)
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _db = _redis.GetDatabase();
        }

        public async Task<string> GetOrAddAsync(string key, Func<string, string> valueFactory, TimeSpan? expiration = null)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            // Try to get from cache first
            var cached = await _db.StringGetAsync(key);
            if (cached.HasValue)
                return cached.ToString();

            // Compute the value
            var value = valueFactory(key);
            if (value != null)
            {
                // Store in cache
                await _db.StringSetAsync(key, value, expiration);
            }

            return value;
        }

        public async Task<string> GetAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            var value = await _db.StringGetAsync(key);
            return value.HasValue ? value.ToString() : null;
        }

        public async Task SetAsync(string key, string value, TimeSpan? expiration = null)
        {
            if (string.IsNullOrEmpty(key))
                return;

            await _db.StringSetAsync(key, value ?? string.Empty, expiration);
        }

        public async Task RemoveAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;

            await _db.KeyDeleteAsync(key);
        }
    }
}
