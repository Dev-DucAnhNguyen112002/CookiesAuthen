using CookiesAuthen.Domain.Entities.Identity;

namespace CookiesAuthen.Domain.Entities;
public class Department : BaseAuditableEntity
{
    public string Name { get; set; } = default!; // Ví dụ: Phòng Kế Toán
    public string Code { get; set; } = default!; // Ví dụ: ACC01
    public string? Description { get; set; } // Mô tả chức năng nhiệm vụ

    public string? ManagerId { get; set; }
    public virtual ApplicationUser? Manager { get; set; }

    public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

    public Guid? ParentId { get; set; }
    public virtual Department? Parent { get; set; }
    public virtual ICollection<Department> Children { get; set; } = new List<Department>();

    public bool IsActive { get; set; } = true;
}
