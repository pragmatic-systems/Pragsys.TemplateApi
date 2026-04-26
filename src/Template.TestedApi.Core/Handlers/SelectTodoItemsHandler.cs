using MediatR;
using Microsoft.EntityFrameworkCore;
using Template.TestedApi.Database;
using Template.TestedApi.Database.Model;

namespace Template.TestedApi.Core.Handlers;

internal class SelectTodoItemsHandler : IRequestHandler<SelectTodo, IEnumerable<TodoRecord>>
{
    private readonly ApplicationDbContext _dbContext;

    public SelectTodoItemsHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<TodoRecord>> Handle(SelectTodo request, CancellationToken cancellationToken)
    {
        return await _dbContext
            .TodoRecords
            .Where(r => r.Open == true)
            .ToListAsync();
    }
}

public record SelectTodo()
    : IRequest<IEnumerable<TodoRecord>>;
