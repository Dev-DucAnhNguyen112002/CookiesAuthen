using System.Security.Claims;
using CookiesAuthen.Application.Common.Interfaces;
using CookiesAuthen.Application.Common.Models;
using CookiesAuthen.Application.Feature.v1.Departments.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CookiesAuthen.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;
    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
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
                FullName = u.FullName ?? u.UserName, // Nếu ko có tên thật thì lấy username
                Email = u.Email!,
                // Logic lấy Role hơi phức tạp xíu, tạm thời để string cứng hoặc join bảng
                Role = u.IsHeadOfDepartment ? "Trưởng phòng" : "Nhân viên"
            })
            .ToListAsync();

        return users;
    }
    public async Task<bool> UpdateUserDepartmentAsync(string userId, int departmentId, bool isHead)
    {
        // 1. Tìm user theo Id
        var user = await _userManager.FindByIdAsync(userId);
        // Nếu không thấy user -> Trả về false
        if (user == null) return false;

        // 2. Cập nhật thông tin
        user.DepartmentId = departmentId;
        user.IsHeadOfDepartment = isHead;

        // 3. Lưu xuống Database
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }
    
}
