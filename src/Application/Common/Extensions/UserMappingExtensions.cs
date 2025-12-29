using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CookiesAuthen.Application.Common.Interfaces;
using CookiesAuthen.Application.Feature.v1.System.Queries;
using CookiesAuthen.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace CookiesAuthen.Application.Common.Extensions;
public static class UserMappingExtensions
{
    public static IQueryable<UserWithPermissionsDto> ProjectToDto(
        this IQueryable<ApplicationUser> query,
        IApplicationDbContext context)
    {
        var userRoles = context.Set<IdentityUserRole<string>>();
        var roles = context.Set<IdentityRole>();
        var roleClaims = context.Set<IdentityRoleClaim<string>>();

        return query.Select(user => new UserWithPermissionsDto
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
        });
    }
}
