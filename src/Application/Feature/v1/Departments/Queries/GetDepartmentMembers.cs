using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CookiesAuthen.Application.Common.Interfaces;
using CookiesAuthen.Application.Feature.v1.Departments.Models;

namespace CookiesAuthen.Application.Feature.v1.Departments.Queries;
public record GetDepartmentMembersQuery(int DepartmentId) : IRequest<List<DepartmentMemberDto>>;

public class GetDepartmentMembersQueryHandler : IRequestHandler<GetDepartmentMembersQuery, List<DepartmentMemberDto>>
{
    private readonly IIdentityService _identityService;

    // Inject Interface vào (chứ không Inject UserManager trực tiếp)
    public GetDepartmentMembersQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<List<DepartmentMemberDto>> Handle(GetDepartmentMembersQuery request, CancellationToken cancellationToken)
    {
        var members = await _identityService.GetUsersByDepartmentAsync(request.DepartmentId);

        return members;
    }
}
