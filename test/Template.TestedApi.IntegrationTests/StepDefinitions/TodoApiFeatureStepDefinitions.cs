using System.Net.Http.Json;
using Newtonsoft.Json;
using Reqnroll;
using Template.TestedApi.Database.Model;
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

        await _testContext.PostAsJsonAsync("todo-list/v1", payload);
    }

    [Then("The response should contain a new RecordId")]
    public async Task TheResponseShouldContainANewRecordId()
    {
        _testContext.NewTodoItem = await _testContext.LastResponse.Content.ReadFromJsonAsync<TodoRecord>();
        _testContext.NewTodoItem.ShouldNotBeNull();
        _testContext.NewTodoItem.ItemId.ShouldNotBe(Guid.Empty);
    }

    [When("We get our TodoList")]
    public async Task WeGetOurTodoList()
    {
        await _testContext.GetAsync("todo-list/v1");
    }

    [Then("The response should contain a Todo List")]
    public async Task TheResponseShouldContainATodoList()
    {
        _testContext.TaskList = await _testContext.LastResponse.Content.ReadFromJsonAsync<List<TodoRecord>>();
        _testContext.TaskList.ShouldNotBeNull();
    }

    [Then("The result contains the created recordId")]
    public void TheResultsContainsTheCreatedItemId()
    {
        var item = _testContext.NewTodoItem;
        var id = item.ItemId;

        var match = _testContext.TaskList
            .SingleOrDefault(i => i.ItemId == id);

        ((object)match).ShouldNotBeNull();
    }
}
