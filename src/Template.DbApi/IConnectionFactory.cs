using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template.DbApi;
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
