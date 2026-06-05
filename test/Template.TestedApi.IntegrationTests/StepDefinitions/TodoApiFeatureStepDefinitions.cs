using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reqnroll;
using Template.TestedApi.IntegrationTests.Infrastructure;

namespace Template.TestedApi.IntegrationTests.StepDefinitions;

[Binding]
public sealed class TodoApiFeatureStepDefinitions
{
    private readonly TestContext _testContext;

    public TodoApiFeatureStepDefinitions(
        TestContext testContext)
    {
        _testContext = testContext;
    }

    [When("We create task '(.*)'")]
    public async Task WeCreateATodoItem(string text)
    {
        var payload = new
        {
            Title = text,
            Description = text,
            DueDate = DateTime.Today,
        };

        await _testContext.PostAsJsonAsync("TodoList", payload);
    }

    [Then("The response should contain a new RecordId")]
    public async Task TheResponseShouldContainANewRecordId()
    {
        var json = await _testContext.LastResponse.Content.ReadAsStringAsync();
        _testContext.NewTodoItem = JsonConvert.DeserializeObject<dynamic>(json);

        ((string)_testContext.NewTodoItem
            .itemId.Value)
            .ShouldNotBeNull();
    }

    [When("We get our TodoList")]
    public async Task WeGetOurTodoList()
    {
        await _testContext.GetAsync("TodoList");
    }

    [Then("The response should contain a Todo List")]
    public async Task TheResponseShouldContainATodoList()
    {
        var json = await _testContext.LastResponse.Content.ReadAsStringAsync();
        _testContext.TaskList = ((JArray)JsonConvert.DeserializeObject<dynamic>(json))
            .Select(j => (dynamic)j)
            .ToList();
    }

    [Then("The result contains the created recordId")]
    public void TheResultsContainsTheCreatedItemId()
    {
        var item = _testContext.NewTodoItem;
        var id = item.itemId;

        var match = _testContext.TaskList
            .SingleOrDefault(i => i.itemId == id);

        ((object)match).ShouldNotBeNull();
    }
}
