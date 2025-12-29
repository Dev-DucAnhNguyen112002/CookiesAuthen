using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace CookiesAuthen.Domain.Entities.Identity;
public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? ManagerId { get; set; } // Sếp trực tiếp là ai
    public virtual ApplicationUser? Manager { get; set; } // Đối tượng sếp (để lấy tên sếp, email sếp...)
    public virtual Department? Department { get; set; }
    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
    public bool IsDirector { get; set; } = false; // Giám đốc (Trùm cuối)
    public bool IsHeadOfDepartment { get; set; } = false; // Trưởng phòng
}
public class ApplicationRole : IdentityRole
{
    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
}

// 2. Class trung gian nối User và Role (Đây là class bạn đang bị thiếu)
public class ApplicationUserRole : IdentityUserRole<string>
{
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual ApplicationRole Role { get; set; } = null!;
}
