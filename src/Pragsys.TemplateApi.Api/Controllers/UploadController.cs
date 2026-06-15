using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Pragsys.TemplateApi.Instrumentation;

namespace Pragsys.TemplateApi.Api.Controllers;

[ApiController]
[Route("upload")]
public class UploadController : ControllerBase
{
    private readonly BlobContainerClient _blobContainerClient;

    public UploadController(BlobContainerClient blobContainerClient)
    {
        _blobContainerClient = blobContainerClient;
    }

    [HttpPost("csv")]
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
        var blobClient = _blobContainerClient.GetBlobClient(blobName);

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
