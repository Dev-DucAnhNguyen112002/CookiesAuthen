using System.Security.Claims;
using System.Text.Encodings.Web;
using CookiesAuthen.Infrastructure.Identity;
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
}
public record TwoFactorResponse
{
    public string? SharedKey { get; set; }
    public string? QrCodeUri { get; set; }
}
