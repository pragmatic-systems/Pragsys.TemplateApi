using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using Dapper;
using System.Threading.Tasks;
using Template.DbApi.Model;
using MediatR;
using System.Collections.Generic;
using Template.DbApi.Handlers;

namespace Template.DbApi.Api.Controllers
{
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
        public async Task<IActionResult> GetItems()
        {
            var result = await _mediator.Send(new SelectTodo());

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> InsertItem(InsertTodo insert)
        {
            var result = await _mediator.Send(insert);
            return Ok(result);
        }
    }
}