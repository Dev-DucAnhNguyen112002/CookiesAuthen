namespace CookiesAuthen.Web.Endpoints;

using CookiesAuthen.Application.Feature.v1.Author.Commands;
public class Author : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapPost(GrantPermission, "/grant-permission")
            .MapPost(RevokePermission, "/revoke-permission");
    }

    public async Task<IResult> GrantPermission(ISender sender, GrantPermissionCommand command)
    {
        // Endpoint không hề biết logic xử lý là gì, chỉ biết gửi đi
        await sender.Send(command);
        return Results.Ok("Cấp quyền thành công");
    }

    public async Task<IResult> RevokePermission(ISender sender, RevokePermissionCommand command)
    {
        await sender.Send(command);
        return Results.Ok("Thu hồi quyền thành công");
    }
}
