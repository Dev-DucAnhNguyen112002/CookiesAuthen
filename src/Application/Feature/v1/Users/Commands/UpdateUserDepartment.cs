using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CookiesAuthen.Application.Common.Interfaces;

namespace CookiesAuthen.Application.Feature.v1.Users.Commands;
public record UpdateUserDepartmentCommand(string UserId, int DepartmentId, bool IsHead) : IRequest;

public class UpdateUserDepartmentCommandHandler : IRequestHandler<UpdateUserDepartmentCommand>
{
    private readonly IIdentityService _identityService; 

    public UpdateUserDepartmentCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task Handle(UpdateUserDepartmentCommand request, CancellationToken cancellationToken)
    {
        var succeeded = await _identityService.UpdateUserDepartmentAsync(request.UserId, request.DepartmentId, request.IsHead);

        // Nếu hàm trả về false (nghĩa là không tìm thấy User ID đó)
        if (!succeeded)
        {
            // Ném ra ngoại lệ NotFound (Global Exception Handler sẽ bắt và trả về 404)
            // Lưu ý: NotFoundException thường nằm trong folder Common/Exceptions
            throw new NotFoundException("User", request.UserId);
        }
    }
}
