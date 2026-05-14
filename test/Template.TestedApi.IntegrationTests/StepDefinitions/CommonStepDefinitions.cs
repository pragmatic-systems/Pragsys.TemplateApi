using System.Net;
using System.Security.Claims;
using Reqnroll;
using Template.TestedApi.Api;
using Template.TestedApi.IntegrationTests.Infrastructure;

namespace Template.TestedApi.IntegrationTests.StepDefinitions;

[Binding]
public class CommonStepDefinitions
{
    private readonly TestContext _testContext;

    public CommonStepDefinitions(TestContext testContext)
    {
        _testContext = testContext;
    }

    [Then("The response should be 200 OK")]
    public void TheResultShouldBeOk()
    {
        _testContext.LastResponse
             .Should().NotBeNull()
             .And
             .Subject.EnsureSuccessStatusCode();
    }

    [Then("The response should be 401 Unauthorized")]
    public void TheResultShouldBeUnauthroized()
    {
        _testContext.LastResponse
             .Should().NotBeNull()
             .And
             .HaveStatusCode(HttpStatusCode.Unauthorized);
    }

    [Then("The response should be 403 Forbidden")]
    public void TheResultShouldBeForbidden()
    {
        _testContext.LastResponse
             .Should().NotBeNull()
             .And
             .HaveStatusCode(HttpStatusCode.Forbidden);
    }

    [Given("We have user '(.*)'")]
    public void WeHaveUser(string userName)
    {
        _testContext.AddUser(userName);
    }

    [Given("User '(.*)' has claims '(.*)'")]
    public void UserHasClaims(string userName, string claimSetName)
    {
        var claims = new List<Claim>();

        if (claimSetName == "Read")
        {
            claims.Add(new Claim(ClaimTypes.Role, Roles.TodoListRead));
        }

        if (claimSetName == "ReadWrite")
        {
            claims.Add(new Claim(ClaimTypes.Role, Roles.TodoListRead));
            claims.Add(new Claim(ClaimTypes.Role, Roles.TodoListWrite));
        }

        _testContext.AddUserClaims(userName, claims);
    }

    [When("We are connecting as '(.*)'")]
    public void WeAreConnectingAsUser(string userName)
    {
        _testContext.SetCurrentUser(userName);
    }

    [When("We are connecting anonymously")]
    public void WeAreConnectingAnonymously()
    {
        _testContext.ClearCurrentUser();
    }
}
