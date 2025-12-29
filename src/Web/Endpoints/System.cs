namespace CookiesAuthen.Web.Endpoints;
using CookiesAuthen.Application.Common.Security;
using CookiesAuthen.Application.Feature.v1.System.Commands;
using CookiesAuthen.Web.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authorization; // Thêm dòng này để dùng AuthorizeAttribute chuẩn
using CookiesAuthen.Application.Common.Interfaces;
using CookiesAuthen.Application.Feature.v1.System.Queries;
using CookiesAuthen.Application.Feature.v1.Departments.Queries;

public class System : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization(new Microsoft.AspNetCore.Authorization.AuthorizeAttribute
            {
                Roles = "Administrator,Director,HeadOfDepartment"
            })
            .MapPost(GrantPermission, "/grant-permission")
            .MapPost(RevokePermission, "/revoke-permission")
            .MapPost(AssignRole, "/assign-role")
            .MapGet(GetDepartmentMembers, "DepartmentMembers");
        var group = app.MapGroup(this).RequireAuthorization();

        // Xem quyền của Role: /api/permissions/role/SALE
        group.MapGet("/role/{roleName}", GetRolePermissions);

        // Xem quyền của User: /api/permissions/user/guid-id
        group.MapGet("user", GetUserPermissions);
    }

    public async Task<IResult> GrantPermission(ISender sender, GrantPermissionCommand command)
    {
        await sender.Send(command);
        return Results.Ok("Cấp quyền thành công");
    }

    public async Task<IResult> RevokePermission(ISender sender, RevokePermissionCommand command)
    {
        await sender.Send(command);
        return Results.Ok("Thu hồi quyền thành công");
    }
    public async Task<IResult> AssignRole(ISender sender, AssignUserRoleCommand command)
    {
        await sender.Send(command);
        return Results.Ok("Đã bổ nhiệm nhân viên vào vị trí thành công!");
    }
    // Thay vì truyền IPermissionService, ta dùng ISender
    public async Task<IResult> GetRolePermissions(string roleName, ISender sender)
    {
        var result = await sender.Send(new GetRolesWithPermissionsQuery { Keyword = roleName });
        return TypedResults.Ok(result);
    }

    public async Task<IResult> GetUserPermissions([AsParameters] GetUsersWithPermissionsQuery query, ISender sender)
    {
        // Tận dụng Query phân trang đã viết để lấy danh sách User kèm quyền
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }
    public async Task<IResult> GetDepartmentMembers([AsParameters] GetDepartmentMembersQuery query, ISender sender)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }
}
