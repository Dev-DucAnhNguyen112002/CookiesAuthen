using CookiesAuthen.Application.Common.Models;
using CookiesAuthen.Application.Feature.v1.Departments.Models;
using CookiesAuthen.Application.Feature.v1.System.Queries;

namespace CookiesAuthen.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);

    Task<Result> DeleteUserAsync(string userId);
    Task<List<DepartmentMemberDto>> GetUsersByDepartmentAsync(int departmentId);
    Task<bool> UpdateUserDepartmentAsync(string userId, int departmentId, bool isHead);
    //Task<List<RoleResponseDto>> GetRolesAsync();
    Task<IList<string>> GetUserRolesAsync(string userId);
    Task<PaginatedList<UserWithPermissionsDto>> GetUsersWithPermissionsAsync(int pageNumber, int pageSize, string? keyword); 
    Task<PaginatedList<RoleDto>> GetRolesWithPermissionsAsync(int pageNumber, int pageSize, string? keyword);
}
