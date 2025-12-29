using System.Security.Claims;

using CookiesAuthen.Application.Common.Interfaces;

namespace CookiesAuthen.Web.Services;

public class CurrentUser : IUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? Id => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public bool IsInRole(string roleName)
    {
        return _httpContextAccessor.HttpContext?.User?.IsInRole(roleName) ?? false;
    }
}
