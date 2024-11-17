using DbUp;
using Npgsql;
using Polly;
using System.Reflection;

public static class Program
{
    static int Main(string[] args)
    {
        var connectionString =
            args.FirstOrDefault()
            ?? Environment.GetEnvironmentVariable("DbConnectionString");

        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentException("Must supply either connection string arg, or DbConnectionString environment variable.");

        var retryPolicy = Policy
            .Handle<NpgsqlException>()
            .WaitAndRetry(20, i => TimeSpan.FromSeconds(5),
                (e, t) => Console.WriteLine("Retrying... Waiting for database"));

        retryPolicy.Execute(() =>
            EnsureDatabase.For.PostgresqlDatabase(connectionString));

        var upgrader =
            DeployChanges.To
                .PostgresqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .LogToConsole()
                .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(result.Error);
            Console.ResetColor();
            return -1;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Success!");
        Console.ResetColor();
        return 0;
    }
}