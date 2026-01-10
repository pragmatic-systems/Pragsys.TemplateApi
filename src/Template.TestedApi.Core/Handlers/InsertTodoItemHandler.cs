using MediatR;
using Template.TestedApi.Database;
using Template.TestedApi.Database.Model;

namespace Template.TestedApi.Core.Handlers;
internal class InsertTodoItemHandler : IRequestHandler<InsertTodo, TodoRecord>
{
    private readonly ApplicationDbContext _applicationDbContext;

    public InsertTodoItemHandler(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async Task<TodoRecord> Handle(InsertTodo request, CancellationToken cancellationToken)
    {
        var record = new TodoRecord(
            Guid.NewGuid(),
            request.Title,
            request.Description,
            request.DueDate);

        _applicationDbContext.TodoRecords.Add(record);

        await _applicationDbContext.SaveChangesAsync();

        return record;
    }
}

public record InsertTodo(string Title, string Description, DateTime DueDate) : IRequest<TodoRecord>;
