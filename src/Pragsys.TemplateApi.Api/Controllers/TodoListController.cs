using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Pragsys.CQRS;
using Pragsys.TemplateApi.Core.Handlers;
using Pragsys.TemplateApi.Instrumentation;

namespace Pragsys.TemplateApi.Api.Controllers;

[ApiController]
[Route("todo-list")]
public class TodoListController : ControllerBase
{
    private readonly IMediator _mediator;
    private BlobContainerClient _blobClient;

    public TodoListController(IMediator mediator, BlobContainerClient blobClient)
    {
        _mediator = mediator;
        _blobClient = blobClient;
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

    [HttpPost("v1/upload")]
    [Authorize(Policy = Roles.TodoListWrite)]
    [EnableRateLimiting("Basic")]
    public async Task<IActionResult> UploadCsv(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Only CSV files are supported.");
        }

        var blobName = $"csv-imports/{Guid.NewGuid()}-{file.FileName}";

        using var stream = file.OpenReadStream();
        var blobClient = _blobClient.GetBlobClient(blobName);

        await blobClient.UploadAsync(stream, true);

        // Queue the background job to process the CSV
        BackgroundJob.Enqueue<CsvImportJobHandler>(
            handler => handler.ImportCsv(blobName));

        return Ok(new
        {
            BlobName = blobName,
            Message = "CSV uploaded and queued for import."
        });
    }
}
