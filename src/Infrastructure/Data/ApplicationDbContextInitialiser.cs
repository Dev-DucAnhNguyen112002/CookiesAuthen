using System.Security.Claims;
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
        // 1. Tạo Phòng ban (Nếu chưa có)
        if (!_context.Set<Department>().Any())
        {
            _context.Set<Department>().AddRange(new List<Department>
        {
            new Department { Name = "Phòng IT", Code = "IT" },
            new Department { Name = "Phòng Nhân Sự", Code = "HR" },
            new Department { Name = "Phòng Kinh Doanh", Code = "SALE" }
        });
            await _context.SaveChangesAsync();
        }

        // Lấy Id phòng ra để gán
        var itDept = await _context.Set<Department>().FirstAsync(d => d.Code == "IT");
        var hrDept = await _context.Set<Department>().FirstAsync(d => d.Code == "HR");

        // 2. Tạo User Giám Đốc (Trùm cuối)
        var directorEmail = "giamdoc@company.com";
        if (_userManager.Users.All(u => u.UserName != directorEmail))
        {
            var director = new ApplicationUser
            {
                UserName = directorEmail,
                Email = directorEmail,
                FullName = "Nguyễn Văn Giám Đốc",
                IsDirector = true // <--- QUAN TRỌNG
            };
            await _userManager.CreateAsync(director, "Password123!");
            await _userManager.AddToRoleAsync(director, "Administrator"); // Cho full quyền chức năng
        }

        // 3. Tạo Trưởng phòng IT
        var tpItEmail = "tpit@company.com";
        if (_userManager.Users.All(u => u.UserName != tpItEmail))
        {
            var tpIt = new ApplicationUser
            {
                UserName = tpItEmail,
                Email = tpItEmail,
                FullName = "Trần Trưởng Phòng IT",
                DepartmentId = itDept.Id, // Thuộc phòng IT
                IsHeadOfDepartment = true // <--- Là sếp
            };
            await _userManager.CreateAsync(tpIt, "Password123!");
            // Gán claim CRUD cơ bản
            await _userManager.AddClaimAsync(tpIt, new Claim("Permission", "Permissions.Weather.Create"));
            await _userManager.AddClaimAsync(tpIt, new Claim("Permission", "Permissions.Weather.Delete"));
        }

        // 4. Tạo Nhân viên IT (Lính)
        var nvItEmail = "nvit@company.com";
        if (_userManager.Users.All(u => u.UserName != nvItEmail))
        {
            var nvIt = new ApplicationUser
            {
                UserName = nvItEmail,
                Email = nvItEmail,
                FullName = "Lê Nhân Viên IT",
                DepartmentId = itDept.Id, // Thuộc phòng IT
                IsHeadOfDepartment = false // <--- Là lính
            };
            await _userManager.CreateAsync(nvIt, "Password123!");
            // Chỉ cho quyền Xem và Tạo (Không cho Xóa)
            await _userManager.AddClaimAsync(nvIt, new Claim("Permission", "Permissions.Weather.Create"));
            // Lưu ý: Không add claim Delete cho ông này
        }

        // ... Tương tự tạo cho phòng HR để test chéo ...
    }
}
