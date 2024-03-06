
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Utility;

namespace ManagerBack.Repositories;

public class CachedCardRepository : ICachedCardRepository
{
    private readonly IDistributedCache _cache;
    public CachedCardRepository(IDistributedCache cache) {
        _cache = cache;
    }
    
    private static string ToKey(string cid) => $"card-{cid}";

    public async Task Forget(string cid)
    {
        await _cache.RemoveAsync(ToKey(cid));
    }

    public async Task<CardModel?> Get(string cid)
    {
        var key = ToKey(cid);
        var data = await _cache.GetStringAsync(key);
        if (string.IsNullOrEmpty(data)) return null;
        return JsonSerializer.Deserialize<CardModel>(data, Common.JSON_SERIALIZATION_OPTIONS);
    }

    public async Task Remember(CardModel card)
    {
        var key = ToKey(card.GetCID());
        await _cache.SetStringAsync(key, JsonSerializer.Serialize(card, Common.JSON_SERIALIZATION_OPTIONS));
    }
}