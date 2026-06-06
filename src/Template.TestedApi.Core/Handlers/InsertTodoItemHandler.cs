using Pragsys.CQRS;
using Template.TestedApi.Database;
using Template.TestedApi.Database.Model;

namespace Template.TestedApi.Core.Handlers;

public class InsertTodoItemHandler : IRequestHandler<InsertTodo, TodoRecord>
{
    private readonly ApplicationDbContext _dbContext;

    public InsertTodoItemHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TodoRecord> Handle(InsertTodo request, CancellationToken cancellationToken)
    {
        var record = new TodoRecord(
            Guid.NewGuid(),
            request.Title,
            request.Description,
            request.DueDate);

        _dbContext.TodoRecords.Add(record);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return record;
    }
}

public record InsertTodo(string? Title, string? Description, DateTime DueDate)
    : IRequest<TodoRecord>;
