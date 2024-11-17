using Dapper;
using MediatR;
using Microsoft.AspNetCore.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.DbApi.Model;

namespace Template.DbApi.Handlers;
internal class InsertTodoItemHandler : IRequestHandler<InsertTodo, TodoRecord>
{
    private readonly IConnectionFactory _connectionFactory;

    public InsertTodoItemHandler(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<TodoRecord> Handle(InsertTodo request, CancellationToken cancellationToken)
    {
        using var conn = _connectionFactory.CreateWriteConnection();

        var recordId = await conn.ExecuteScalarAsync<Guid>("INSERT INTO todo_list (title, description, due_date) VALUES (@Title, @Description, @DueDate) RETURNING item_id;", request);

        var record = new TodoRecord(
            recordId,
            request.Title,
            request.Description,
            request.DueDate);

        return record;
    }
}

public record InsertTodo(string Title, string Description, DateTime DueDate) : IRequest<TodoRecord>;
