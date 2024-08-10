using Dapper;
using NewPoint.Handlers;
using NewPoint.Models;

namespace NewPoint.Repositories;

internal class ImageRepository : IImageRepository
{
    public readonly string TableName = @"""image""";

    public async Task<bool> ImageExists(long id)
    {
        var counter = await DatabaseHandler.Connection.ExecuteScalarAsync<int>(@$"
        SELECT COUNT(1) FROM {TableName}
        WHERE id=@id;
        ",
            new { id });

        return counter != 0;
    }

    public async Task<int> Count(string name)
    {
        var counter = await DatabaseHandler.Connection.ExecuteScalarAsync<int>(@$"
        SELECT COUNT(1) FROM {TableName}
        WHERE name=@name;
        ",
            new { name });

        return counter;
    }

    public async Task<string> GetImageNameById(long id)
    {
        var name = await DatabaseHandler.Connection.QueryFirstAsync<string>(@$"
        SELECT name FROM {TableName}
        WHERE id=@id;
        ",
            new { id });

        return name;
    }

    public async Task<long> GetImageIdByName(string name)
    {
        var id = await DatabaseHandler.Connection.QueryFirstAsync<long>(@$"
        SELECT id FROM {TableName}
        WHERE name=@name;
        ",
            new { name });

        return id;
    }

    public async Task<long> InsertImage(string name)
    {
        var id = await DatabaseHandler.Connection.ExecuteScalarAsync<long>(@$"
        INSERT INTO {TableName} (name)
        VALUES (@name)
        RETURNING id;
        ",
            new
            {
                name = name
            });
        return id;
    }
}

public interface IImageRepository
{
    Task<bool> ImageExists(long id);
    Task<int> Count(string name);
    Task<string> GetImageNameById(long id);
    Task<long> GetImageIdByName(string name);
    Task<long> InsertImage(string name);
}