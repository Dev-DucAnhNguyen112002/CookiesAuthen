// src/Infrastructure/Caching/RedisCacheService.cs
using System.Text.Json; // Dùng để biến Object thành String
using CookiesAuthen.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed; // Thư viện chuẩn của .NET

namespace CookiesAuthen.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;

    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var cachedData = await _cache.GetStringAsync(key, cancellationToken);
        if (string.IsNullOrEmpty(cachedData))
        {
            return default;
        }
        // Redis lưu dạng String/Json -> Phải Deserialize ngược lại thành Object
        return JsonSerializer.Deserialize<T>(cachedData);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            // Mặc định lưu 10 phút nếu không truyền vào
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(10)
        };

        var jsonData = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, jsonData, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);
    }
}
