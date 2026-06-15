using System;
using System.Linq;
using System.Threading.Tasks;
using Pragsys.TemplateApi.IntegrationTests.Infrastructure;
using Reqnroll;

namespace Pragsys.TemplateApi.IntegrationTests.StepDefinitions;

[Binding]
public sealed class CsvUploadImportStepDefinitions
{
    private readonly TestContext _testContext;

    public CsvUploadImportStepDefinitions(TestContext testContext)
    {
        _testContext = testContext;
    }

    [When(@"We upload a CSV file with three todo items")]
    public async Task WeUploadACsvFileWithThreeTodoItems()
    {
        var csvContent = @"Buy groceries,Weekly shopping list,2025-12-25
Walk the dog,Daily evening walk,
Read a book,Finish the current novel,2025-12-31";

        await _testContext.UploadCsvAsync("test-todos.csv", csvContent);
    }

    [Then("The response should contain a blob name")]
    public void TheResponseShouldContainABlobName()
    {
        _testContext.UploadedBlobName.ShouldNotBeNull();
    }

    [Then("The blob name should not be empty")]
    public void TheBlobNameShouldNotBeEmpty()
    {
        _testContext.UploadedBlobName.ShouldNotBeNullOrEmpty();
    }

    [When("We wait for the background job to complete")]
    public async Task WeWaitForTheBackgroundJobToComplete()
    {
        // Hangfire jobs are queued and processed asynchronously.
        // We give the worker time to pick up and process the job.
        await Task.Delay(TimeSpan.FromSeconds(5));
    }

    [Then("The response should contain at least {int} todo items")]
    public void TheResponseShouldContainAtLeastNTodoItems(int count)
    {
        _testContext.TodoList.ShouldNotBeNull();
        _testContext.TodoList.Count.ShouldBeGreaterThanOrEqualTo(count);
    }
}
