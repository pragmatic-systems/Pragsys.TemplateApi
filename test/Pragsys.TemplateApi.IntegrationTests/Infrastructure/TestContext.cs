using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Polly;
using Pragsys.TemplateApi.Database.Model;
using Pragsys.TemplateApi.IntegrationTests.Infrastructure.Auth;

namespace Pragsys.TemplateApi.IntegrationTests.Infrastructure;

public class TestContext
{
    private readonly TestRuntime _testRuntime;

    public TestContext(TestRuntime testRuntime)
    {
        SigningCertificate = testRuntime.SigningCertificate;
        _testRuntime = testRuntime;
    }

    public TodoRecord NewTodoItem { get; set; }

    public List<TodoRecord>? TodoList { get; set; }

    public Dictionary<string, TestUser> Users { get; private set; } = new Dictionary<string, TestUser>();

    public HttpResponseMessage? LastResponse { get; set; }

    public AsyncPolicy RetryPolicy { get; } = Policy
        .Handle<HttpRequestException>()
        .WaitAndRetryAsync(10, i => TimeSpan.FromSeconds(1));

    public PemCertificate? SigningCertificate { get; internal set; }

    public TestUser? CurrentUser { get; private set; }

    public async Task GetAsync(string path)
    {
        using var client = _testRuntime.SubjectApi.CreateClient();
        LastResponse = await RetryPolicy.ExecuteAsync(async () =>
        {
            if (CurrentUser != null)
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CurrentUser.UserJwt);

            var result = await client.GetAsync(path);
            return result;
        });
    }

    public async Task PostAsJsonAsync<T>(string path, T payload)
    {
        using var client = _testRuntime.SubjectApi.CreateClient();
        LastResponse = await RetryPolicy.ExecuteAsync(async () =>
        {
            if (CurrentUser != null)
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CurrentUser.UserJwt);

            var result = await client.PostAsJsonAsync(path, payload);
            return result;
        });
    }

    public void AddUser(string userName)
    {
        var user = new TestUser(userName);
        Users.Add(userName, user);
    }

    public void AddUserClaims(string userName, IEnumerable<Claim> claims)
    {
        Users[userName].Claims.AddRange(claims);
    }

    public void SetCurrentUser(string userName)
    {
        var user = Users[userName];
        CurrentUser = user;
        CurrentUser.BuildJwt(SigningCertificate, _testRuntime.JwtIssuer);
    }

    public void ClearCurrentUser()
    {
        CurrentUser = null;
    }
}
