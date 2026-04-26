using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Template.TestedApi.Core.Handlers;

namespace Template.TestedApi.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class TodoListController : ControllerBase
{
    private readonly IMediator _mediator;

    public TodoListController(IMediator store)
    {
        _mediator = store;
    }

    [HttpGet]
    [Authorize(Policy = Roles.TodoListRead)]
    public async Task<IActionResult> GetItems()
    {
        var result = await _mediator.Send(new SelectTodo());
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = Roles.TodoListWrite)]
    public async Task<IActionResult> InsertItem(InsertTodo insert)
    {
        var result = await _mediator.Send(insert);
        return Ok(result);
    }
}
