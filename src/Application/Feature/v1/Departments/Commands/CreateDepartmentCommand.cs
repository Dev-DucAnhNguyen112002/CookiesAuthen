using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CookiesAuthen.Application.Common.Interfaces;
using CookiesAuthen.Domain.Entities;

namespace CookiesAuthen.Application.Feature.v1.Departments.Commands;
public record CreateDepartmentCommand : IRequest<Guid>
{
    public string Name { get; init; } = default!;
    public string Code { get; init; } = default!;
    public string? Description { get; init; }
    public Guid? ParentId { get; init; }
    public string? ManagerId { get; init; }
}

public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateDepartmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var entity = new Department
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            ParentId = request.ParentId,
            ManagerId = request.ManagerId,
            IsActive = true
        };

        _context.Set<Department>().Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
