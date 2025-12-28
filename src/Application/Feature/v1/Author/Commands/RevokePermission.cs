using CookiesAuthen.Application.Common.Interfaces; // Nơi chứa IIdentityService
using CookiesAuthen.Application.Common.Security;   // Nơi chứa Authorize Attribute

namespace CookiesAuthen.Application.Feature.v1.Author.Commands;

// 1. Định nghĩa Command (Input)
// Chỉ Administrator mới được phép thu hồi quyền
[Authorize(Roles = "Administrator")]
public record RevokePermissionCommand(string RoleName, string Permission) : IRequest;

// 2. Định nghĩa Handler (Xử lý logic)
public class RevokePermissionCommandHandler : IRequestHandler<RevokePermissionCommand>
{
    private readonly IPermissionService _IPermissionService;

    public RevokePermissionCommandHandler(IPermissionService identityService)
    {
        _IPermissionService = identityService;
    }

    public async Task Handle(RevokePermissionCommand request, CancellationToken cancellationToken)
    {
        // Gọi Service ở tầng Infrastructure để thực hiện xóa claim khỏi DB
        var result = await _IPermissionService.RevokePermissionAsync(request.RoleName, request.Permission);

        // Kiểm tra kết quả
        if (!result.Succeeded)
        {
            throw new ValidationException(string.Join("; ", result.Errors));
        }
    }
}
