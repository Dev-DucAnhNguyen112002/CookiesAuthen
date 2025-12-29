using System.Security.Claims;
using CookiesAuthen.Application.Common.Security;
using CookiesAuthen.Domain.Constants;
using CookiesAuthen.Domain.Entities;
using CookiesAuthen.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CookiesAuthen.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();

        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // ==============================================================================
        // 1. TẠO ROLES (CÁC PHÒNG BAN)
        // ==============================================================================
        await EnsureRoleAsync("Administrator");
        await EnsureRoleAsync("IT");
        await EnsureRoleAsync("SALE");
        await EnsureRoleAsync("HR");
        await EnsureRoleAsync("Director");
        await EnsureRoleAsync("HeadOfDepartment");
        // ==============================================================================
        // 2. PHÂN QUYỀN (DÙNG ENUM)
        // ==============================================================================

        // --- PHÒNG IT: Chỉ được quyền XEM Weather ---
        await GrantPermissionEnumAsync("IT", ResourceType.WeatherForecast, PermissionAction.View);

        // --- PHÒNG SALE: Được TẠO và IMPORT Weather ---
        await GrantPermissionEnumAsync("SALE", ResourceType.WeatherForecast, PermissionAction.Create);
        await GrantPermissionEnumAsync("SALE", ResourceType.WeatherForecast, PermissionAction.Import); // (Ví dụ mở rộng)

        // --- PHÒNG HR: Được XÓA Weather ---
        await GrantPermissionEnumAsync("HR", ResourceType.WeatherForecast, PermissionAction.Delete);

        // --- ADMINISTRATOR: FULL QUYỀN (Tự động lặp qua tất cả Enum để cấp hết) ---
        await GrantFullAccessToRoleAsync("Administrator");

        // ==============================================================================
        // 3. TẠO USER MẪU (SEEDING USERS)
        // ==============================================================================

        // Admin
        await EnsureUserAsync("admin@localhost", "Administrator1!", "Administrator");

        // Nhân viên IT
        await EnsureUserAsync("it@localhost", "Password123!", "IT");

        // Nhân viên Sale
        await EnsureUserAsync("sale@localhost", "Password123!", "SALE");

        // Nhân viên HR
        await EnsureUserAsync("hr@localhost", "Password123!", "HR");

    }
    private async Task EnsureRoleAsync(string roleName)
    {
        if (_roleManager.Roles.All(r => r.Name != roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    private async Task EnsureUserAsync(string email, string password, string roleName)
    {
        if (_userManager.Users.All(u => u.UserName != email))
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
                // Có thể set thêm DepartmentId nếu muốn test logic Department Owner
            };

            await _userManager.CreateAsync(user, password);
            await _userManager.AddToRoleAsync(user, roleName);
        }
    }

    // Hàm quan trọng: Chuyển đổi Enum -> String Claim và lưu vào DB
    private async Task GrantPermissionEnumAsync(string roleName, ResourceType resource, PermissionAction action)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null) return;

        // Tạo chuỗi claim chuẩn: "Permissions.WeatherForecast.View"
        string permissionString = $"Permissions.{resource}.{action}";

        var allClaims = await _roleManager.GetClaimsAsync(role);

        // Kiểm tra xem có chưa, chưa có mới thêm
        if (!allClaims.Any(a => a.Type == "Permission" && a.Value == permissionString))
        {
            await _roleManager.AddClaimAsync(role, new Claim("Permission", permissionString));
        }
    }

    // Hàm cấp Full quyền cho Admin (Duyệt qua tất cả Resource và Action)
    private async Task GrantFullAccessToRoleAsync(string roleName)
    {
        // Duyệt qua từng Resource (Weather, Product, User...)
        foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
        {
            // Duyệt qua từng Action (View, Create, Delete...)
            foreach (PermissionAction action in Enum.GetValues(typeof(PermissionAction)))
            {
                // Bỏ qua các giá trị cờ gộp hoặc None để tránh rác DB
                if (action != PermissionAction.None &&
                    action != PermissionAction.FullAccess &&
                    action != PermissionAction.ViewEdit)
                {
                    await GrantPermissionEnumAsync(roleName, resource, action);
                }
            }
        }
    }
}
