using CookiesAuthen.Application.Common.Interfaces;
using CookiesAuthen.Application.Common.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace CookiesAuthen.Infrastructure.Identity;

public class PermissionService : IPermissionService
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public PermissionService(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
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

        if (claim == null) return Result.Failure(new[] { "Permission not found." });

        var result = await _roleManager.RemoveClaimAsync(role, claim);
        return result.ToApplicationResult();
    }
}
