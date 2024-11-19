using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Template.TestedApi.IntegrationTests.Infrastructure;

namespace Template.TestedApi.IntegrationTests.StepDefinitions;

[Binding]
public sealed class TodoApiFeatureStepDefinitions
{
    private readonly CoreTestContext _testContext;
    private readonly TestContext _todoListTestContext;

    public TodoApiFeatureStepDefinitions(
        CoreTestContext testContext,
        TestContext todoListTestContext)
    {
        _testContext = testContext;
        _todoListTestContext = todoListTestContext;
    }

    [When("We create task '(.*)'")]
    public async Task WeCreateATodoItem(string text)
    {
        var payload = new
        {
            Title = text,
            Description = text,
            DueDate = DateTime.Today
        };

        await _testContext.PostAsJsonAsync("TodoList", payload);
    }

    [Then("The response should contain a new RecordId")]
    public async Task TheResponseShouldContainANewRecordId()
    {
        var json = await _testContext.Response.Content.ReadAsStringAsync();
        _todoListTestContext.NewTodoItem = JsonConvert.DeserializeObject<dynamic>(json);

        ((string)_todoListTestContext.NewTodoItem
            .itemId.Value)
            .Should().NotBeNull();
    }

    [When("We get our TodoList")]
    public async Task WeGetOurTodoList()
    {
        await _testContext.GetAsync("TodoList");
    }

    [Then("The response should contain a Todo List")]
    public async Task TheResponseShouldContainATodoList()
    {
        var json = await _testContext.Response.Content.ReadAsStringAsync();
        _todoListTestContext.TaskList = ((JArray)JsonConvert.DeserializeObject<dynamic>(json))
            .Select(j => (dynamic)j)
            .ToList();
    }

    [Then("The result contains the created recordId")]
    public void TheResultsContainsTheCreatedItemId()
    {
        var item = _todoListTestContext.NewTodoItem;
        var id = item.itemId;

        var match = _todoListTestContext.TaskList
            .SingleOrDefault(i => i.itemId == id);

        ((object)match).Should().NotBeNull();
    }
}
