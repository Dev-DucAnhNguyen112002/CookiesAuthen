using System.Security.Claims;
using CookiesAuthen.Application.Common.Interfaces;
using CookiesAuthen.Application.Common.Models;
using CookiesAuthen.Application.Feature.v1.Departments.Models;
using CookiesAuthen.Application.Feature.v1.System.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CookiesAuthen.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IApplicationDbContext _context;
    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService,
        RoleManager<IdentityRole> roleManager,
        IApplicationDbContext context)
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
        _roleManager = roleManager;
        _context = context;
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
    public async Task<List<DepartmentMemberDto>> GetUsersByDepartmentAsync(int departmentId)
    {
        // Đây là nơi duy nhất được phép đụng vào ApplicationUser
        var users = await _userManager.Users
            .Where(u => u.DepartmentId == departmentId)
            .Select(u => new DepartmentMemberDto
            {
                Id = u.Id,
                FullName = u.FullName ?? u.UserName, 
                Email = u.Email!,
                Role = u.IsHeadOfDepartment ? "Trưởng phòng" : "Nhân viên"
            })
            .ToListAsync();

        return users;
    }
    public async Task<bool> UpdateUserDepartmentAsync(string userId, int departmentId, bool isHead)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;
        user.DepartmentId = departmentId;
        user.IsHeadOfDepartment = isHead;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
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
        var query = _userManager.Users.AsNoTracking();

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(u => u.UserName!.Contains(keyword) || u.Email!.Contains(keyword));
        }

        var totalCount = await query.CountAsync();

        // Lấy các tập hợp thực thể thông qua phương thức Set<T>()
        var userRoles = _context.Set<IdentityUserRole<string>>();
        var roles = _context.Set<IdentityRole>();
        var roleClaims = _context.Set<IdentityRoleClaim<string>>();

        var items = await query
            .OrderBy(u => u.UserName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(user => new UserWithPermissionsDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                Roles = userRoles
                    .Where(ur => ur.UserId == user.Id)
                    .Join(roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name!)
                    .ToList(),
                Permissions = userRoles
                    .Where(ur => ur.UserId == user.Id)
                    .Join(roleClaims, ur => ur.RoleId, rc => rc.RoleId, (ur, rc) => rc)
                    .Where(rc => rc.ClaimType == "Permission")
                    .Select(rc => rc.ClaimValue!)
                    .Distinct()
                    .ToList()
            })
            .ToListAsync();

        return new PaginatedList<UserWithPermissionsDto>(items, totalCount, pageNumber, pageSize);
    }
    public async Task<IList<string>> GetUserRolesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return new List<string>();

        return await _userManager.GetRolesAsync(user);
    }
    
}
