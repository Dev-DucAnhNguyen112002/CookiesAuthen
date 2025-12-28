
namespace CookiesAuthen.Application.Feature.v1.Departments.Models;
public class DepartmentMemberDto
{
    public string Id { get; set; } = default!;
    public string? FullName { get; set; }
    public string Email { get; set; } = default!;
    public string Role { get; set; } = default!; // Ví dụ: "Trưởng phòng", "Nhân viên"
}
