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

    [Given("We have a Application api")]
    public void WeHaveAnApplicationApi()
    {
    }

    [Then("The response should be 200 OK")]
    public void TheResultShouldBeOk()
    {
        _testContext.LastResponse
             .Should().NotBeNull()
             .And
             .Subject.EnsureSuccessStatusCode();
    }
}
