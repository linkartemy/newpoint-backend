using Dapper;
using NewPoint.Handlers;
using NewPoint.Models;

namespace NewPoint.Repositories;

internal class ImageRepository : IImageRepository
{
    public async Task<bool> ImageExists(long id)
    {
        var counter = await DatabaseHandler.Connection.ExecuteScalarAsync<int>(@"
        SELECT COUNT(1) FROM ""image""
        WHERE id=@id;
        ",
            new { id });

        return counter != 0;
    }

    public async Task<int> Count(string name)
    {
        var counter = await DatabaseHandler.Connection.ExecuteScalarAsync<int>(@"
        SELECT COUNT(1) FROM ""image""
        WHERE name=@name;
        ",
            new { name });

        return counter;
    }

    public async Task<string> GetImageNameById(long id)
    {
        var name = await DatabaseHandler.Connection.QueryFirstAsync<string>(@"
        SELECT name FROM ""image""
        WHERE id=@id;
        ",
            new { id });

        return name;
    }

    public async Task<long> InsertImage(string name)
    {
        var id = await DatabaseHandler.Connection.ExecuteScalarAsync<long>(@"
        INSERT INTO ""image"" (name)
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
    Task<long> InsertImage(string name);
}