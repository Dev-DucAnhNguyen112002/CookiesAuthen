using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CookiesAuthen.Application.Common.Interfaces;
using CookiesAuthen.Application.Common.Models;

namespace CookiesAuthen.Application.Feature.v1.System.Queries;
public record RoleDto(string Id, string Name, List<string> Permissions);

public record GetRolesWithPermissionsQuery : PaginationRequest, IRequest<PaginatedList<RoleDto>>;

public class GetRolesWithPermissionsQueryHandler : IRequestHandler<GetRolesWithPermissionsQuery, PaginatedList<RoleDto>>
{
    private readonly IIdentityService _identityService;

    public GetRolesWithPermissionsQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<PaginatedList<RoleDto>> Handle(GetRolesWithPermissionsQuery request, CancellationToken cancellationToken)
    {
        return await _identityService.GetRolesWithPermissionsAsync(
            request.PageNumber,
            request.PageSize,
            request.Keyword);
    }
}
