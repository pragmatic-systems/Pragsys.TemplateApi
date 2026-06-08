using Microsoft.EntityFrameworkCore;
using Pragsys.CQRS;
using Pragsys.TemplateApi.Database;
using Pragsys.TemplateApi.Database.Model;

namespace Pragsys.TemplateApi.Core.Handlers;

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
