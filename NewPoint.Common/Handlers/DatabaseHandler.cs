using System.Data;
using Npgsql;

namespace NewPoint.Common.Handlers;

public static class DatabaseHandler
{
    public static string ConnectionString { private get; set; } = string.Empty;

    public static IDbConnection Connection
        => new NpgsqlConnection(ConnectionString);
}