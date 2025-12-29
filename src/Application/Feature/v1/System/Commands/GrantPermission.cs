
using CookiesAuthen.Application.Common.Interfaces;
using CookiesAuthen.Application.Common.Security;
namespace CookiesAuthen.Application.Feature.v1.System.Commands;
// Command nằm ở Application
[Authorize(Roles = "Administrator")]
public record GrantPermissionCommand(string RoleName, ResourceType Resource,
    PermissionAction Action) : IRequest;

public class GrantPermissionCommandHandler : IRequestHandler<GrantPermissionCommand>
{
    private readonly IPermissionService _IPermissionService;

    public GrantPermissionCommandHandler(IPermissionService identityService)
    {
        _IPermissionService = identityService;
    }

    public async Task Handle(GrantPermissionCommand request, CancellationToken cancellationToken)
    {
        foreach (PermissionAction singleAction in Enum.GetValues(typeof(PermissionAction)))
        {
            if (singleAction == PermissionAction.None ||
                singleAction == PermissionAction.FullAccess ||
                singleAction == PermissionAction.ViewEdit) continue;

            if (request.Action.HasFlag(singleAction))
            {
                string permissionString = $"Permissions.{request.Resource}.{singleAction}";

                // Gọi Service: Dù có rồi hay chưa cũng đều trả về Success
                var result = await _IPermissionService.GrantPermissionAsync(request.RoleName, permissionString);

                // Nếu có lỗi khác (VD: Role không tìm thấy) thì mới ném lỗi
                if (!result.Succeeded)
                {
                    // throw new ValidationException(result.Errors);
                }
            }
        }
    }
}
