using Polly;
using System.Net.Http.Json;
using Template.TestedApi.IntegrationTests.Infrastructure.Jwt;

namespace Template.TestedApi.IntegrationTests.Infrastructure;

public class TestContext
{
    public dynamic NewTodoItem { get; set; }

    public List<dynamic> TaskList { get; set; }

    public HttpClient TestClient { get;internal set; }

    public HttpResponseMessage LastResponse { get; set; }

    public AsyncPolicy RetryPolicy { get; } = Policy
        .Handle<HttpRequestException>()
        .WaitAndRetryAsync(10, i => TimeSpan.FromSeconds(1));

    public async Task GetAsync(string path)
    {
        LastResponse = await RetryPolicy.ExecuteAsync(async () =>
        {
            var result = await TestClient.GetAsync(path);
            result.EnsureSuccessStatusCode();
            return result;
        });
    }

    public async Task PostAsJsonAsync<T>(string path, T payload)
    {
        LastResponse = await RetryPolicy.ExecuteAsync(async () =>
        {
            var result = await TestClient.PostAsJsonAsync(path, payload);
            result.EnsureSuccessStatusCode();
            return result;
        });
    }
}
