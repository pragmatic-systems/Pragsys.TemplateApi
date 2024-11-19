namespace Template.TestedApi.IntegrationTests.Infrastructure;

[Binding]
public class RuntimeSetup
{
    public static readonly TestRuntime TestRuntime = new TestRuntime();

    public static async Task ConfigureRuntime()
    {
        await TestRuntime.InitializeAsync();
    }

    public static async Task DisposeRuntime()
    {
        await TestRuntime.DisposeAsync();
    }
}

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