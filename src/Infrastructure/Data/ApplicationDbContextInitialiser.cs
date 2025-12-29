using System.Security.Claims;
using CookiesAuthen.Application.Common.Security;
using CookiesAuthen.Domain.Entities;
using CookiesAuthen.Domain.Entities.Identity;
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
    private readonly RoleManager<ApplicationRole> _roleManager;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
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
        // 1. Khởi tạo Roles
        await EnsureRoleAsync("Administrator");
        await EnsureRoleAsync("IT");
        await EnsureRoleAsync("SALE");
        await EnsureRoleAsync("HR");
        await EnsureRoleAsync("Director");
        await EnsureRoleAsync("HeadOfDepartment");

        // 2. Khởi tạo Departments (Phòng ban) mẫu
        if (!_context.Set<Department>().Any())
        {
            _context.Set<Department>().AddRange(
                new Department { Name = "Ban Giám Đốc", Code = "BGD", IsActive = true },
                new Department { Name = "Phòng CNTT", Code = "IT", IsActive = true },
                new Department { Name = "Phòng Kinh Doanh", Code = "SALE", IsActive = true },
                new Department { Name = "Phòng Nhân Sự", Code = "HR", IsActive = true }
            );
            await _context.SaveChangesAsync();
        }

        // 3. Phân quyền cho các Role
        await GrantPermissionEnumAsync("IT", ResourceType.WeatherForecast, PermissionAction.View);
        await GrantPermissionEnumAsync("SALE", ResourceType.WeatherForecast, PermissionAction.Create);
        await GrantPermissionEnumAsync("HR", ResourceType.WeatherForecast, PermissionAction.Delete);

        // Cấp full quyền cho Admin và Director
        await GrantFullAccessToRoleAsync("Administrator");
        await GrantFullAccessToRoleAsync("Director");

        var itDept = _context.Set<Department>().FirstOrDefault(d => d.Code == "IT");
        var bgdDept = _context.Set<Department>().FirstOrDefault(d => d.Code == "BGD");

        await EnsureUserAsync("admin@localhost", "Administrator1!", "Administrator", bgdDept?.Id);
        await EnsureUserAsync("director@localhost", "Director1!", "Director", bgdDept?.Id);
        await EnsureUserAsync("it@localhost", "Password123!", "IT", itDept?.Id);
    }
    private async Task EnsureRoleAsync(string roleName)
    {
        if (_roleManager.Roles.All(r => r.Name != roleName))
        {
            // Sử dụng {} thay vì ()
            await _roleManager.CreateAsync(new ApplicationRole { Name = roleName });
        }
    }

    private async Task EnsureUserAsync(string email, string password, string roleName, Guid? departmentId = null)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DepartmentId = departmentId,
                // Tự động set flag dựa trên role
                IsDirector = roleName == "Director" || roleName == "Administrator",
                IsHeadOfDepartment = roleName == "HeadOfDepartment"
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
