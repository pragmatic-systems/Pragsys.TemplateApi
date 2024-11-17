using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Template.DbApi.AcceptanceTests;
public class CoreTestContext
{
    public string Uri { get; set; }

    public HttpResponseMessage Response { get; set; }

    public AsyncPolicy RetryPolicy { get; } = Policy
        .Handle<HttpRequestException>()
        .WaitAndRetryAsync(10, i => TimeSpan.FromSeconds(1));

    public HttpClient CreateClient()
    {
        return new HttpClient()
        {
            BaseAddress = new Uri(Uri)
        };
    }

    public async Task GetAsync(string path) 
    {
        using var client = CreateClient();

        Response = await RetryPolicy.ExecuteAsync(async () => {
            var result = await client.GetAsync(path);
            result.EnsureSuccessStatusCode();
            return result;
        });
    }

    public async Task PostAsJsonAsync<T>(string path, T payload)
    {
        using var client = CreateClient();

        Response = await RetryPolicy.ExecuteAsync(async () => {
            var result = await client.PostAsJsonAsync(path, payload);
            result.EnsureSuccessStatusCode();
            return result;
        });
    }
}
