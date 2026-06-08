using Pragsys.CQRS;
using Pragsys.TemplateApi.Database;
using Pragsys.TemplateApi.Database.Model;

namespace Pragsys.TemplateApi.Core.Handlers;

public class InsertTodoItemHandler : IRequestHandler<InsertTodo, TodoRecord>
{
    private readonly ApplicationDbContext _dbContext;

    public InsertTodoItemHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TodoRecord> Handle(InsertTodo request, CancellationToken cancellationToken)
    {
        var record = TodoRecord.Create(
            Guid.NewGuid(),
            request.Title,
            request.Description,
            request.DueDate.ToUniversalTime());

        _dbContext.TodoRecords.Add(record);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return record;
    }
}

public record InsertTodo(string? Title, string? Description, DateTime DueDate)
    : IRequest<TodoRecord>;
