using Newtonsoft.Json;
using StackExchange.Redis;

namespace Yggdrasil.Redis;

public static class RedisExtensions
{
    public static Task<bool> Set<T>(this IDatabase db, string key, T value)
    {
        return db.StringSetAsync(key, JsonConvert.SerializeObject(value));
    }
    
    public static T Get<T>(this IDatabase db, string key)
    {
        var data = db.StringGet(key);
        if (data.IsNullOrEmpty)
        {
            return default;
        }
        return JsonConvert.DeserializeObject<T>(data);
    }
}