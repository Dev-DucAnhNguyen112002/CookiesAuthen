
using CookiesAuthen.Application.Common.Interfaces;
using CookiesAuthen.Application.Common.Security;
namespace CookiesAuthen.Application.Feature.v1.Author.Commands;

// Command nằm ở Application
[Authorize(Roles = "Administrator")]
public record GrantPermissionCommand(string RoleName, string Permission) : IRequest;

// Handler nằm cùng file (hoặc cùng folder) ở Application
public class GrantPermissionCommandHandler : IRequestHandler<GrantPermissionCommand>
{
    private readonly IPermissionService _IPermissionService;

    public GrantPermissionCommandHandler(IPermissionService identityService)
    {
        _IPermissionService = identityService;
    }

    public async Task Handle(GrantPermissionCommand request, CancellationToken cancellationToken)
    {
        var result = await _IPermissionService.GrantPermissionAsync(request.RoleName, request.Permission);
        if (!result.Succeeded) throw new ValidationException(string.Join("; ", result.Errors));
    }
}
