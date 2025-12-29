
using CookiesAuthen.Application.Common.Interfaces;
using CookiesAuthen.Application.Common.Security;

namespace CookiesAuthen.Application.Feature.v1.System.Commands;
[Authorize(Roles = "Administrator,Director")]
public record AssignUserRoleCommand(string UserId, string RoleName) : IRequest;

public class AssignUserRoleCommandHandler : IRequestHandler<AssignUserRoleCommand>
{
    private readonly IPermissionService _IPermissionService; // Nhớ dùng Interface này

    public AssignUserRoleCommandHandler(IPermissionService identityService)
    {
        _IPermissionService = identityService;
    }

    public async Task Handle(AssignUserRoleCommand request, CancellationToken cancellationToken)
    {
        // Bạn cần bổ sung hàm này vào IIdentityService nhé
        var result = await _IPermissionService.AssignUserToRoleAsync(request.UserId, request.RoleName);

        if (!result.Succeeded)
        {
            throw new ValidationException(string.Join("; ", result.Errors));
        }
    }
}
