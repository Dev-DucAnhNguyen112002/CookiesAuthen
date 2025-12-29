using CookiesAuthen.Application.Common.Interfaces; // Nơi chứa IIdentityService
using CookiesAuthen.Application.Common.Security;   // Nơi chứa Authorize Attribute

namespace CookiesAuthen.Application.Feature.v1.System.Commands;

// 1. Định nghĩa Command (Input)
// Chỉ Administrator mới được phép thu hồi quyền
[Authorize(Roles = "Administrator,Director")]
public record RevokePermissionCommand(string RoleName, ResourceType Resource,
    PermissionAction Action) : IRequest;

// 2. Định nghĩa Handler (Xử lý logic)
public class RevokePermissionCommandHandler : IRequestHandler<RevokePermissionCommand>
{
    private readonly IPermissionService _permissionService;

    public RevokePermissionCommandHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public async Task Handle(RevokePermissionCommand request, CancellationToken cancellationToken)
    {
        // Duyệt qua tất cả các quyền đơn lẻ (View, Create, Update...)
        foreach (PermissionAction singleAction in Enum.GetValues(typeof(PermissionAction)))
        {
            // Bỏ qua None, FullAccess, ViewEdit (chỉ xử lý quyền đơn lẻ)
            if (singleAction == PermissionAction.None ||
                singleAction == PermissionAction.FullAccess ||
                singleAction == PermissionAction.ViewEdit)
            {
                continue;
            }

            // CHECK BITWISE: Kiểm tra xem lệnh thu hồi có chứa quyền này không?
            if (request.Action.HasFlag(singleAction))
            {
                // Tạo chuỗi permission chuẩn: "Permissions.WeatherForecast.Create"
                string permissionString = $"Permissions.{request.Resource}.{singleAction}";

                // Gọi Service xóa khỏi DB
                // (Nhờ Bước 1, nếu quyền này không có sẵn thì nó vẫn báo Success và chạy tiếp)
                var result = await _permissionService.RevokePermissionAsync(request.RoleName, permissionString);

                if (!result.Succeeded)
                {
                    // Nếu lỗi hệ thống (VD: mất kết nối DB) thì mới throw
                    //throw new ValidationException(result.Errors);
                }
            }
        }
    }
}
