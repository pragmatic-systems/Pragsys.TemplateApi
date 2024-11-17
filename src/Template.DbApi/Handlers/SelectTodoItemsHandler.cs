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
internal class SelectTodoItemsHandler : IRequestHandler<SelectTodo, IEnumerable<TodoRecord>>
{
    private readonly IConnectionFactory _connectionFactory;

    public SelectTodoItemsHandler(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<TodoRecord>> Handle(SelectTodo request, CancellationToken cancellationToken)
    {
        using var conn = _connectionFactory.CreateReadConnection();

        var items = await conn.QueryAsync("SELECT item_id, title, description, due_date, open, closed_date FROM todo_list WHERE open = true;");

        // NOTE: This will require pagination to scale.
        return items
            .Select(d => new TodoRecord(d.item_id, d.title, d.description, d.due_date, d.open, d.closed_date))
            .ToList();
    }
}

public record SelectTodo() : IRequest<IEnumerable<TodoRecord>>;
