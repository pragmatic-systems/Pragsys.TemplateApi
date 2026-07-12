using Azure.Storage.Blobs;
using Microsoft.Extensions.Hosting;

namespace Pragmatic.TemplateApi.Instrumentation.HostedServices;

public class BlobContainerInitService : IHostedService
{
    private readonly BlobContainerClient _blobContainerClient;

    public BlobContainerInitService(BlobContainerClient blobContainerClient)
    {
        _blobContainerClient = blobContainerClient;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _blobContainerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
