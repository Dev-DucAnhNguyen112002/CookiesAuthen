using System;

using CookiesAuthen.Application.Common.Models;

namespace CookiesAuthen.Application.Common.Interfaces;
public interface IPermissionService
{
    Task<Result> GrantPermissionAsync(string roleName, string permission);
    Task<Result> RevokePermissionAsync(string roleName, string permission);
}
