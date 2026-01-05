using System.Security.Claims;
using System.Text.Encodings.Web;
using CookiesAuthen.Application.Common.Messages;
using CookiesAuthen.Application.Feature.v1.Users.Models;
using CookiesAuthen.Application.Feature.v1.Users.Queries;
using CookiesAuthen.Domain.Entities.Identity;
using CookiesAuthen.Infrastructure.Identity;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CookiesAuthen.Web.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapIdentityApi<ApplicationUser>();
        app.MapGroup(this)
            .RequireAuthorization() // Bắt buộc đăng nhập
            .MapGet("/2fa_setup", GetTwoFactorSetup);
        app.MapGroup(this).AllowAnonymous().MapGet(GetListUser,"/GetListUser");
        app.MapPost("/api/test-queue", async (IPublishEndpoint publishEndpoint) =>
        {
            // IPublishEndpoint là service của MassTransit dùng để BẮN tin
            await publishEndpoint.Publish(new UserCreatedEvent
            {
                UserId = Guid.NewGuid().ToString(),
                Email = "test@gmail.com"
            });

            return Results.Ok("Đã bắn tin nhắn vào hàng đợi! Check log đi!");
        }).AllowAnonymous();
    }

    public async Task<Ok<TwoFactorResponse>> GetTwoFactorSetup(
        ClaimsPrincipal claimsUser,
        [FromServices] UserManager<ApplicationUser> userManager) // Thêm [FromServices] cho chắc ăn
    {
        var user = await userManager.GetUserAsync(claimsUser);

        // Lưu ý: Nếu user null, technically nên return NotFound, 
        // nhưng để đơn giản cho hàm Ok<T>, ta giả định user luôn có (vì đã RequireAuthorization)
        // Hoặc nếu muốn return NotFound, bạn phải đổi kiểu về Results<Ok<T>, NotFound>

        var key = await userManager.GetAuthenticatorKeyAsync(user!); // user! vì chắc chắn không null
        if (string.IsNullOrEmpty(key))
        {
            await userManager.ResetAuthenticatorKeyAsync(user!);
            key = await userManager.GetAuthenticatorKeyAsync(user!);
        }

        var email = user!.Email;
        var appName = "MyCleanApp";

        var authenticatorUri = string.Format(
            "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6",
            UrlEncoder.Default.Encode(appName),
            UrlEncoder.Default.Encode(email!),
            key);

        // Dùng TypedResults.Ok
        return TypedResults.Ok(new TwoFactorResponse{ SharedKey = key, QrCodeUri = authenticatorUri });
    }
    public async Task<List<UserDto>> GetListUser([FromServices] ISender sender,[AsParameters] GetUsersQuery request)
    {
        var result = await sender.Send(request);
        return result;
    }
}
public record TwoFactorResponse
{
    public string? SharedKey { get; set; }
    public string? QrCodeUri { get; set; }
}
