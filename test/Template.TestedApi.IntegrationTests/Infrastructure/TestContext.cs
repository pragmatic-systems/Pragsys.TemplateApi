using Polly;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Template.TestedApi.Api;
using Template.TestedApi.IntegrationTests.Infrastructure.Jwt;
using Template.TestedApi.IntegrationTests.Infrastructure.OpenId;

namespace Template.TestedApi.IntegrationTests.Infrastructure;

public class TestContext
{
    private TestRuntime TestRuntime;

    public TestContext(TestRuntime testRuntime)
    {
        TestClient = testRuntime.TargetApi.CreateClient();
        SigningCertificate = testRuntime.SigningCertificate;
        TestRuntime = testRuntime;
    }

    public dynamic NewTodoItem { get; set; }

    public List<dynamic> TaskList { get; set; }

    public Dictionary<string, TestUser> Users { get; private set; } = new Dictionary<string, TestUser>();

    public HttpClient TestClient { get; internal set; }

    public HttpResponseMessage LastResponse { get; set; }

    public AsyncPolicy RetryPolicy { get; } = Policy
        .Handle<HttpRequestException>()
        .WaitAndRetryAsync(10, i => TimeSpan.FromSeconds(1));

    public PemCertificate SigningCertificate { get; internal set; }

    public TestUser CurrentUser { get; private set; }

    public async Task GetAsync(string path)
    {
        LastResponse = await RetryPolicy.ExecuteAsync(async () =>
        {
            if (CurrentUser != null)
                TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CurrentUser.UserJwt);

            var result = await TestClient.GetAsync(path);
            return result;
        });
    }

    public async Task PostAsJsonAsync<T>(string path, T payload)
    {
        LastResponse = await RetryPolicy.ExecuteAsync(async () =>
        {
            if (CurrentUser != null)
                TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CurrentUser.UserJwt);

            var result = await TestClient.PostAsJsonAsync(path, payload);
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
        CurrentUser.BuildJwt(SigningCertificate);
    }

    public void ClearCurrentUser()
    {
        CurrentUser = null;
    }
}

public class TestUser
{
    public string UserName { get; private set; }

    public List<Claim> Claims { get; private set; } = new List<Claim>();

    public string UserJwt { get; private set; }

    public TestUser(string userName)
    {
        UserName = userName;
    }

    public void BuildJwt(PemCertificate certificate)
    {
        var audience = TestConstants.Audience;
        var issuer = TestConstants.Issuer;
        var signingCertificate = certificate.ToX509Certificate2();

        var accessTokenParameters = new AccessTokenParameters(audience, issuer, signingCertificate, Claims);
        UserJwt = JwtBearerAccessTokenFactory.Create(accessTokenParameters);
    }
}