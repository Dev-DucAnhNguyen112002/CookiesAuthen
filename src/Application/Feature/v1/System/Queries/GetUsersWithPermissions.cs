using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CookiesAuthen.Application.Common.Interfaces;
using CookiesAuthen.Application.Common.Models;

namespace CookiesAuthen.Application.Feature.v1.System.Queries;
public class UserWithPermissionsDto
{
    public string Id { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public IList<string> Roles { get; set; } = new List<string>();
    public List<string> Permissions { get; set; } = new List<string>();
    public UserWithPermissionsDto() { }

    public UserWithPermissionsDto(string id, string userName, string email, IList<string> roles, List<string> permissions)
    {
        Id = id;
        UserName = userName;
        Email = email;
        Roles = roles;
        Permissions = permissions;
    }
}
public record GetUsersWithPermissionsQuery : PaginationRequest, IRequest<PaginatedList<UserWithPermissionsDto>>;


public class GetUsersWithPermissionsQueryHandler : IRequestHandler<GetUsersWithPermissionsQuery, PaginatedList<UserWithPermissionsDto>>
{
    private readonly IIdentityService _identityService;

    public GetUsersWithPermissionsQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }
    public async Task<PaginatedList<UserWithPermissionsDto>> Handle(GetUsersWithPermissionsQuery request, CancellationToken cancellationToken)
    {
        return await _identityService.GetUsersWithPermissionsAsync(
            request.PageNumber,
            request.PageSize,
            request.Keyword);
    }
}
