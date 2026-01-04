using CookiesAuthen.Application.Common.Interfaces;
using CookiesAuthen.Application.Common.Models; // Nhớ using DTO
using CookiesAuthen.Application.Feature.v1.Users.Models;
using CookiesAuthen.Domain.Entities.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CookiesAuthen.Application.Feature.v1.Users.Queries;

// 👇 1. Khai báo rõ Query này sẽ trả về List<UserDto>
public class GetUsersQuery : IRequest<List<UserDto>>
{
}

// 👇 2. Handler cũng phải khai báo kiểu trả về khớp với Query
public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cache;

    public GetUsersQueryHandler(IApplicationDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    // 👇 3. Sửa Task thành Task<List<UserDto>>
    public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        string cacheKey = "List_All_Users";

        // --- BƯỚC 1: CHECK CACHE ---
        // Ép kiểu về List<UserDto> khi lấy ra
        var cachedUsers = await _cache.GetAsync<List<UserDto>>(cacheKey, cancellationToken);
        if (cachedUsers != null)
        {
            return cachedUsers; // ✅ Cache Hit
        }

        // --- BƯỚC 2: GỌI DATABASE ---
        // Lưu ý: Phải map sang DTO ngay tại đây
        var users = await _context.Set<ApplicationUser>() // Hoặc _context.Set<ApplicationUser>()
            .AsNoTracking() // Tối ưu hiệu năng cho query đọc
            .Select(u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName!,
                Email = u.Email!
            })
            .ToListAsync(cancellationToken);

        // --- BƯỚC 3: LƯU CACHE ---
        // Lưu dữ liệu vừa lấy được vào Redis (Sống trong 5 phút)
        await _cache.SetAsync(cacheKey, users, TimeSpan.FromMinutes(5), cancellationToken);

        return users; // ✅ Cache Miss (Lấy từ DB)
    }
}
