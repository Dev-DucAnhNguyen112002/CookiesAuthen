using System.Security.Claims;
using CookiesAuthen.Application.Common.Interfaces;
using CookiesAuthen.Application.Common.Models;
using CookiesAuthen.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace CookiesAuthen.Infrastructure.Identity;

public class PermissionService : IPermissionService
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public PermissionService(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<Result> GrantPermissionAsync(string roleName, string permission)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null) return Result.Failure(new[] { $"Role '{roleName}' not found." });

        var claims = await _roleManager.GetClaimsAsync(role);
        if (claims.Any(c => c.Type == "Permission" && c.Value == permission))
        {
            return Result.Failure(new[] { "Permission already exists." });
        }

        var result = await _roleManager.AddClaimAsync(role, new Claim("Permission", permission));
        return result.ToApplicationResult();
    }

    public async Task<Result> RevokePermissionAsync(string roleName, string permission)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null) return Result.Failure(new[] { "Role not found." });

        var claims = await _roleManager.GetClaimsAsync(role);
        var claim = claims.FirstOrDefault(c => c.Type == "Permission" && c.Value == permission);

        // SỬA ĐOẠN NÀY:
        // Nếu không tìm thấy quyền này -> Coi như đã xóa xong -> Return Success
        if (claim == null)
        {
            return Result.Success();
        }

        var result = await _roleManager.RemoveClaimAsync(role, claim);
        return result.ToApplicationResult();
    }
    public async Task<Result> AssignUserToRoleAsync(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Result.Failure(new[] { "User not found" });

        var roleExists = await _roleManager.RoleExistsAsync(roleName);
        if (!roleExists) return Result.Failure(new[] { $"Role '{roleName}' not found" });

        // Kiểm tra xem user đã có role này chưa
        if (await _userManager.IsInRoleAsync(user, roleName))
        {
            return Result.Failure(new[] { "User already has this role" });
        }

        // Thực hiện gán Role
        var result = await _userManager.AddToRoleAsync(user, roleName);

        return result.ToApplicationResult();
    }
    public async Task<Result> TransferUserRoleAsync(string userId, string newRoleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Result.Failure(new[] { "User not found" });

        // 1. Kiểm tra Role mới có tồn tại không
        if (!await _roleManager.RoleExistsAsync(newRoleName))
        {
            return Result.Failure(new[] { $"Role '{newRoleName}' not found" });
        }

        // 2. Lấy danh sách các Role hiện tại của User (Ví dụ: SALE, Staff...)
        var currentRoles = await _userManager.GetRolesAsync(user);

        // 3. XÓA HẾT role cũ (Bước quan trọng nhất)
        if (currentRoles.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                return removeResult.ToApplicationResult();
            }
        }

        // 4. THÊM role mới
        var addResult = await _userManager.AddToRoleAsync(user, newRoleName);

        return addResult.ToApplicationResult();
    }
    public async Task<List<string>> GetPermissionsByRoleAsync(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null) return new List<string>();

        var claims = await _roleManager.GetClaimsAsync(role);

        // Lấy tất cả giá trị ClaimValue có Type là Permission
        return claims
            .Where(c => c.Type == "Permission")
            .Select(c => c.Value)
            .ToList();
    }
    public async Task<List<string>> GetPermissionsByUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return new List<string>();

        // 1. Lấy danh sách tên các Role của User (ví dụ: ["SALE", "Staff"])
        var roles = await _userManager.GetRolesAsync(user);

        var allPermissions = new HashSet<string>(); // Dùng HashSet để tránh trùng lặp

        foreach (var roleName in roles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var claims = await _roleManager.GetClaimsAsync(role);
                var permissions = claims
                    .Where(c => c.Type == "Permission")
                    .Select(c => c.Value);

                foreach (var p in permissions) allPermissions.Add(p);
            }
        }

        return allPermissions.ToList();
    }
}
