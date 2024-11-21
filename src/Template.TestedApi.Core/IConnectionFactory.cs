using Npgsql;
using System.Data;

namespace Template.TestedApi.Core;
public interface IConnectionFactory
{
    IDbConnection CreateWriteConnection();

    IDbConnection CreateReadConnection();
}

public class PostgresConnectionFactory : IConnectionFactory
{
    private readonly string _writeConnectionString;
    private readonly string _readConnectionString;

    public PostgresConnectionFactory(string writeConnectionString, string readConnectionString)
    {
        _writeConnectionString = writeConnectionString;
        _readConnectionString = readConnectionString;
    }

    public IDbConnection CreateWriteConnection() => new NpgsqlConnection(_writeConnectionString);
    public IDbConnection CreateReadConnection() => new NpgsqlConnection(_readConnectionString);
}
