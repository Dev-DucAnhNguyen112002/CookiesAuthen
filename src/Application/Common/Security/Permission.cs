namespace CookiesAuthen.Application.Common.Security;

// 1. Định nghĩa Hành động (Dùng Flags để cộng dồn)
[Flags] // Attribute bắt buộc để C# hiểu đây là Bitwise
public enum PermissionAction
{
    None = 0,
    View = 1,      
    Create = 2,   
    Update = 4,   
    Delete = 8,   
    Import = 16,   

    ViewEdit = View | Update, // 5
    FullAccess = View | Create | Update | Delete | Import
}

// 2. Định nghĩa Tài nguyên (Modules)
public enum ResourceType
{
    WeatherForecast,
    Users,
    System,
    Products,
    Departments
}
