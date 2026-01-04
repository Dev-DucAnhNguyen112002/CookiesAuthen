
namespace CookiesAuthen.Application.Common.Interfaces;
public interface ICacheService
{
    // Lấy dữ liệu từ Cache
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    // Lưu dữ liệu vào Cache (kèm thời gian hết hạn)
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    // Xóa Cache
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
