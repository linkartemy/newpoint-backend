using System.Data;
using Npgsql;

namespace NewPointStorage.Handlers;

internal static class DatabaseHandler
{
    public static string ConnectionString { private get; set; } = string.Empty;

    public static IDbConnection Connection
        => new NpgsqlConnection(ConnectionString);
}