using CookiesAuthen.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CookiesAuthen.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public int? DepartmentId { get; set; } // Thuộc phòng nào
    public string? ManagerId { get; set; } // Sếp trực tiếp là ai

    public virtual Department? Department { get; set; }
    public bool IsDirector { get; set; } = false; // Giám đốc (Trùm cuối)
    public bool IsHeadOfDepartment { get; set; } = false; // Trưởng phòng
}
