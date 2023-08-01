using System.Data;
using Npgsql;

namespace NewPoint.Handlers;

internal static class DatabaseHandler
{
    public static string ConnectionString { private get; set; } = string.Empty;

    public static IDbConnection Connection
        => new NpgsqlConnection(ConnectionString);
}