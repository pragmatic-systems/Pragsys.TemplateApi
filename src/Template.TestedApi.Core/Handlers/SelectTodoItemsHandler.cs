using Microsoft.EntityFrameworkCore;
using Pragsys.CQRS;
using Template.TestedApi.Database;
using Template.TestedApi.Database.Model;

namespace Template.TestedApi.Core.Handlers;

public class SelectTodoItemsHandler : IRequestHandler<SelectTodo, IEnumerable<TodoRecord>>
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
            .Where(r => r.Open)
            .ToListAsync();
    }
}

public record SelectTodo()
    : IRequest<IEnumerable<TodoRecord>>;
