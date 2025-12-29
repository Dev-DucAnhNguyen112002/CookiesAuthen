using CookiesAuthen.Application.Common.Security;
using Microsoft.AspNetCore.Builder;

namespace CookiesAuthen.Web.Extensions;

public static class EndpointExtensions
{
    // SỬA HÀM CHO GROUP
    public static RouteGroupBuilder RequirePermission(
        this RouteGroupBuilder group, 
        ResourceType resource, 
        PermissionAction action)
    {
        // Thay vì gọi theo tên string, ta dựng policy tại chỗ bằng Lambda
        return group.RequireAuthorization(policy => 
            policy.RequireClaim("Permission", $"Permissions.{resource}.{action}"));
    }

    // SỬA HÀM CHO ENDPOINT LẺ
    public static RouteHandlerBuilder RequirePermission(
        this RouteHandlerBuilder builder, 
        ResourceType resource, 
        PermissionAction action)
    {
        // Tương tự như trên
        return builder.RequireAuthorization(policy => 
            policy.RequireClaim("Permission", $"Permissions.{resource}.{action}"));
    }
}
