
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Utility;

namespace ManagerBack.Repositories;

public class CachedMatchConfigRepository : ICachedMatchConfigRepository
{
    private readonly IDistributedCache _cache;
    public CachedMatchConfigRepository(IDistributedCache cache) {
        _cache = cache;
    }

    public async Task Forget(string id)
    {
        await _cache.RemoveAsync(ToKey(id));
    }

    public async Task<MatchConfigModel?> Get(string id)
    {
        var key = ToKey(id);
        var data = await _cache.GetStringAsync(key);
        if (string.IsNullOrEmpty(data)) return null;
        return JsonSerializer.Deserialize<MatchConfigModel>(data, Common.JSON_SERIALIZATION_OPTIONS);
    }

    public async Task Remember(MatchConfigModel config)
    {
        var key = ToKey(config.Id!);
        await _cache.SetStringAsync(key, JsonSerializer.Serialize(config, Common.JSON_SERIALIZATION_OPTIONS));
    }

    private static string ToKey(string id) => $"matchconfig-{id}";
}