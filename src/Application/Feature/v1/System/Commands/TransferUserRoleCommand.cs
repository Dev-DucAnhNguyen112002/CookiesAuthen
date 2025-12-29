using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CookiesAuthen.Application.Common.Exceptions;
using CookiesAuthen.Application.Common.Interfaces;
using CookiesAuthen.Application.Common.Security;
using ApplicationException = CookiesAuthen.Application.Common.Exceptions.ValidationException;
namespace CookiesAuthen.Application.Feature.v1.System.Commands;
[Authorize(Roles = "Administrator,Director,HR")]
public record TransferUserRoleCommand(string UserId, string NewRoleName) : IRequest;
public class TransferUserRoleCommandHandler : IRequestHandler<TransferUserRoleCommand>
{
    private readonly IPermissionService _permissionService; // Nhớ dùng Interface này
    private readonly IUser _currentUser;
    public TransferUserRoleCommandHandler(IPermissionService identityService, IUser currentUser)
    {
        _permissionService = identityService;
        _currentUser = currentUser;
    }

    public async Task Handle(TransferUserRoleCommand request, CancellationToken cancellationToken)
    {
        // Bạn cần bổ sung hàm này vào IIdentityService nhé
        if (request.NewRoleName == "Administrator" || request.NewRoleName == "Director")
        {
            if (_currentUser.IsInRole("HR"))
            {
                throw new ForbiddenAccessException("Nhân sự (HR) không có quyền bổ nhiệm Admin hay Giám đốc!");
            }
        }
        var result = await _permissionService.TransferUserRoleAsync(request.UserId, request.NewRoleName);

        if (!result.Succeeded)
        {
            var errors = result.Errors ?? Array.Empty<string>();
            var errorDict = new Dictionary<string, string[]>
        {
            { "System", errors.ToArray() }
        };
            throw new ApplicationException(errorDict);
        }
    }
}
