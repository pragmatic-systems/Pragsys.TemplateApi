using DbUp;
using Npgsql;
using Polly;
using System.Reflection;

namespace Template.TestedApi.Database;

public static class Migrator
{
    public static void Migrate(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentException("connectionString is required.");

        var retryPolicy = Policy
            .Handle<NpgsqlException>()
            .WaitAndRetry(
                10,
                i => TimeSpan.FromSeconds(2),
                (e, t) => Console.WriteLine("Retrying... Waiting for database"));

        retryPolicy.Execute(() =>
            EnsureDatabase.For.PostgresqlDatabase(connectionString));

        retryPolicy.Execute(() =>
            ExecuteMigration(connectionString));
    }

    private static void ExecuteMigration(string connectionString)
    {
        var upgrader =
            DeployChanges.To
                .PostgresqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .LogToConsole()
                .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
            throw result.Error;
    }
}
