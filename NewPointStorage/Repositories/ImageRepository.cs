using Dapper;
using NewPointStorage.Handlers;

namespace NewPointStorage.Repositories;

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

    public async Task<string> GetImageHashById(long id)
    {
        var hash = await DatabaseHandler.Connection.QueryFirstAsync<string>(@"
        SELECT
            hash
        FROM ""image""
        WHERE id=@id;
        ",
            new { id });

        return hash;
    }

    public async Task<long> InsertImage(string hash)
    {
        var id = await DatabaseHandler.Connection.ExecuteScalarAsync<long>(@"
        INSERT INTO ""image"" (hash)
        VALUES (@hash)
        RETURNING id;
        ",
            new
            {
                hash,
            });
        return id;
    }
}

public interface IImageRepository
{
    Task<bool> ImageExists(long id);
    Task<string> GetImageHashById(long id);
    Task<long> InsertImage(string hash);
}