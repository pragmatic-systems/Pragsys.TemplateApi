using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json;
using Polly;
using Pragsys.TemplateApi.Database.Model;
using Pragsys.TemplateApi.IntegrationTests.Infrastructure;
using Reqnroll;

namespace Pragsys.TemplateApi.IntegrationTests.StepDefinitions;

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
        _testContext.TodoList = await _testContext.LastResponse.Content.ReadFromJsonAsync<List<TodoRecord>>();
        _testContext.TodoList.ShouldNotBeNull();
    }

    [Then("The result contains the created recordId")]
    public void TheResultsContainsTheCreatedItemId()
    {
        var item = _testContext.NewTodoItem;
        var id = item.ItemId;

        var match = _testContext.TodoList
            .SingleOrDefault(i => i.ItemId == id);

        ((object)match).ShouldNotBeNull();
    }

    [When(@"We upload a CSV file with three todo items")]
    public async Task WeUploadACsvFileWithThreeTodoItems()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Buy groceries,Weekly shopping list,2025-12-25");
        sb.AppendLine("Walk the dog,Daily evening walk,");
        sb.AppendLine("Read a book,Finish the current novel,2025-12-31");

        await _testContext.UploadCsvAsync("test-todos.csv", sb.ToString());
    }

    [Then("The response should contain a blob name")]
    public void TheResponseShouldContainABlobName()
    {
        _testContext.UploadedBlobName.ShouldNotBeNull();
        _testContext.UploadedBlobName.ShouldNotBeNullOrEmpty();
    }

    [When("We wait for the background job to complete")]
    public async Task WeWaitForTheBackgroundJobToComplete()
    {
        var initialCount = 0;

        await Policy
            .Handle<HttpRequestException>()
            .Or<Exception>()
            .WaitAndRetryAsync(
                Enumerable.Range(0, 15)
                    .Select(i => TimeSpan.FromSeconds(1)),
                async (outcome, delay, retry, ctx) =>
                {
                    await _testContext.GetAsync("todo-list/v1");
                    _testContext.TodoList = await _testContext.LastResponse.Content.ReadFromJsonAsync<List<TodoRecord>>();
                })
            .ExecuteAsync(async () =>
            {
                if (_testContext.TodoList.Count <= initialCount)
                    throw new InvalidOperationException("CSV import not yet complete");

                return Task.CompletedTask;
            });
    }

    [Then("The response should contain at least {int} todo items")]
    public void TheResponseShouldContainAtLeastNTodoItems(int count)
    {
        _testContext.TodoList.ShouldNotBeNull();
        _testContext.TodoList.Count.ShouldBeGreaterThanOrEqualTo(count);
    }
}
