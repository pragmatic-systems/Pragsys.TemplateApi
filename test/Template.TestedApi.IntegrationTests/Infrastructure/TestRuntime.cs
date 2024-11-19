namespace Template.TestedApi.IntegrationTests.Infrastructure;

public class TestRuntime : IAsyncDisposable
{
    //private PostgreSqlContainer PostgresContainer;
    //private WireMockContainer WireMockContainer;

    //public WebApplicationFactory<Program> TargetApi { get; private set; }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        // TODO: 
    }
}