using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Newtonsoft.Json;
using System;

namespace TIMApi
{
    public class RedisCacheService
    {
        private RedisCacheOptions options;

        public RedisCacheService()
        {
            options = new RedisCacheOptions
            {
                Configuration = "127.0.0.1:6379",
                InstanceName = ""
            };
        }

        public T Get<T>(string cacheKey)
        {
            using (var redisCache = new RedisCache(options))
            {
                var valueString = redisCache.GetString(cacheKey);
                if (!string.IsNullOrEmpty(valueString))
                {
                    var valueObject = JsonConvert.DeserializeObject<T>(valueString);
                    return (T)valueObject;
                }
                return default;
            }
        }
        public void Set(string cacheKey, object valueObject, int expiration)
        {
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expiration)
            };
            using (var redisCache = new RedisCache(options))
            {
                var valueString = JsonConvert.SerializeObject(valueObject);
                redisCache.SetString(cacheKey, valueString, cacheOptions);
            }
        }

    }
}
