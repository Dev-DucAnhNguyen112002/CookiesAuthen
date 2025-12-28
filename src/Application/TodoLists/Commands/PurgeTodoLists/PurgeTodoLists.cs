using CookiesAuthen.Application.Common.Interfaces;
using CookiesAuthen.Application.Common.Security;
using CookiesAuthen.Domain.Constants;
using CookiesAuthen.Domain.Entities;

namespace CookiesAuthen.Application.TodoLists.Commands.PurgeTodoLists;

[Authorize(Roles = Roles.Administrator)]
[Authorize(Policy = Policies.CanPurge)]
public record PurgeTodoListsCommand : IRequest;

public class PurgeTodoListsCommandHandler : IRequestHandler<PurgeTodoListsCommand>
{
    private readonly IApplicationDbContext _context;

    public PurgeTodoListsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(PurgeTodoListsCommand request, CancellationToken cancellationToken)
    {
        _context.Set<TodoList>().RemoveRange(_context.Set<TodoList>());

        await _context.SaveChangesAsync(cancellationToken);
    }
}
