using System.Security.Claims;
using CookiesAuthen.Application.Common.Interfaces;
using CookiesAuthen.Application.Common.Interfaces.Repository;
using CookiesAuthen.Application.Common.Models;
using CookiesAuthen.Application.Feature.v1.Departments.Models;
using CookiesAuthen.Application.Feature.v1.System.Queries;
using CookiesAuthen.Domain.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CookiesAuthen.Application.Common.Extensions;
namespace CookiesAuthen.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IApplicationDbContext _context;
    private readonly IRepository<ApplicationUser> _userRepository; // Đổi tên cho rõ nghĩa
    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService,
        RoleManager<ApplicationRole> roleManager,
        IApplicationDbContext context,
        IRepository<ApplicationUser> userRepository)
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
        _roleManager = roleManager;
        _context = context;
        _userRepository = userRepository;
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password)
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = userName,
        };

        var result = await _userManager.CreateAsync(user, password);

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }

    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        var result = await _userManager.DeleteAsync(user);

        return result.ToApplicationResult();
    }
    public async Task<PaginatedList<RoleDto>> GetRolesWithPermissionsAsync(int pageNumber, int pageSize, string? keyword)
    {
        var query = _roleManager.Roles.AsNoTracking();

        if (!string.IsNullOrEmpty(keyword))
            query = query.Where(r => r.Name!.Contains(keyword));

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RoleDto(
                r.Id,
                r.Name!,
                _context.Set<IdentityRoleClaim<string>>()
                    .Where(rc => rc.RoleId == r.Id && rc.ClaimType == "Permission")
                    .Select(rc => rc.ClaimValue!)
                    .ToList()
            ))
            .ToListAsync(); // Gửi 1 câu SQL duy nhất

        return new PaginatedList<RoleDto>(items, totalCount, pageNumber, pageSize);
    }
    public async Task<PaginatedList<UserWithPermissionsDto>> GetUsersWithPermissionsAsync(int pageNumber, int pageSize, string? keyword)
    {
        // Bước 1: Query gốc từ UserManager (Kiểu IQueryable<ApplicationUser>)
        var query = _userManager.Users.AsNoTracking();

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(u => u.UserName!.Contains(keyword) || u.Email!.Contains(keyword));
        }

        var projectedQuery = query.OrderBy(u => u.UserName).ProjectToDto(_context);

        // Bước 4: Phân trang (Lúc này projectedQuery đã là kiểu DTO rồi)
        return await _userRepository.GetPagedListAsync<UserWithPermissionsDto>(
            query: projectedQuery,
            pageIndex: pageNumber,
            pageSize: pageSize
        );
    }
    public async Task<IList<string>> GetUserRolesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return new List<string>();

        return await _userManager.GetRolesAsync(user);
    }
    
}
