
namespace CookiesAuthen.Application.Common.Security;

public class AuthorizePermissionAttribute : AuthorizeAttribute
{
    // Constructor nhận vào Enum
    public AuthorizePermissionAttribute(ResourceType resource, PermissionAction action)
    {
        // Tự động sinh ra chuỗi Policy chuẩn: "Permissions.WeatherForecast.View"
        // Điều này giúp Code trong Controller sạch, còn bên dưới vẫn tương thích với Identity
        Policy = $"Permissions.{resource}.{action}";
    }
}
