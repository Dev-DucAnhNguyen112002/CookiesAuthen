using CookiesAuthen.Application.Common.Security;
using CookiesAuthen.Application.Feature.v1.Departments.Commands;
using CookiesAuthen.Web.Extensions;

namespace CookiesAuthen.Web.Endpoints;

public class Department : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup("api/v1/departments")
                       .WithTags("Departments")
                       .RequireAuthorization(); // Phải đăng nhập

        // Chỉ Admin hoặc người có quyền Department.Create (Giám đốc) mới được gọi
        group.MapPost("/", CreateDepartment)
             .RequirePermission(ResourceType.Departments, PermissionAction.Create);
    }

    public static async Task<IResult> CreateDepartment(
        ISender sender,
        CreateDepartmentCommand command)
    {
        var id = await sender.Send(command);
        return Results.Created($"/api/v1/departments/{id}", id);
    }
}
