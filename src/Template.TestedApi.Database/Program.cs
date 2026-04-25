namespace Template.TestedApi.Database;

public static class Program
{
    public static int Main(string[] args)
    {
        var connectionString =
            args.FirstOrDefault()
            ?? Environment.GetEnvironmentVariable("DbConnectionString");

        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentException("Must supply either connection string arg, or DbConnectionString environment variable.");

        try
        {
            Migrator.Migrate(connectionString);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex);
            Console.ResetColor();
            return -1;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Success!");
        Console.ResetColor();
        return 0;
    }
}
