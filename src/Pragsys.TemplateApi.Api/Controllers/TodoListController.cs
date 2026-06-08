using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Pragsys.CQRS;
using Pragsys.TemplateApi.Core.Handlers;

namespace Pragsys.TemplateApi.Api.Controllers;

[ApiController]
[Route("todo-list")]
public class TodoListController : ControllerBase
{
    private readonly IMediator _mediator;

    public TodoListController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("v1")]
    [Authorize(Policy = Roles.TodoListRead)]
    [EnableRateLimiting("Basic")]
    public async Task<IActionResult> GetItems()
    {
        var result = await _mediator.Send(new SelectTodo());
        return Ok(result);
    }

    [HttpPost("v1")]
    [Authorize(Policy = Roles.TodoListWrite)]
    [EnableRateLimiting("Basic")]
    public async Task<IActionResult> InsertItem(InsertTodo insert)
    {
        var result = await _mediator.Send(insert);
        return Ok(result);
    }
}
