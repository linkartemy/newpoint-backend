using Dapper;
using NewPoint.Handlers;
using NewPoint.Models;

namespace NewPoint.Repositories;

public class PostShareRepository : IPostShareRepository
{
    public static string TableName = "post_share";

    public async Task<long> AddPostShare(long userId, long postId)
    {
        var id = await DatabaseHandler.Connection.ExecuteScalarAsync<long>($@"
        INSERT INTO {TableName} (user_id, post_id, creation_timestamp)
        VALUES (@userId, @postId, now())
        RETURNING id;
        ",
            new
            {
                userId,
                postId
            });
        return id;
    }

    public async Task<long> GetPostIdById(long id)
    {
        var postId = await DatabaseHandler.Connection.ExecuteScalarAsync<long>($@"
        SELECT post_id FROM {TableName} WHERE id = @id;
        ",
            new
            {
                id
            });
        return postId;
    }

    public async Task<long> CountPostShares(long userId, long postId)
    {
        var count = await DatabaseHandler.Connection.ExecuteScalarAsync<long>($@"
        SELECT COUNT(*) FROM {TableName} WHERE user_id = @userId AND post_id = @postId;
        ",
            new
            {
                userId,
                postId
            });
        return count;
    }

    public async Task DeletePostShareByPostId(long postId)
    {
        await DatabaseHandler.Connection.ExecuteAsync($@"
        DELETE FROM {TableName} WHERE post_id = @postId;
        ",
            new
            {
                postId
            });
    }
}

public interface IPostShareRepository
{
    Task<long> AddPostShare(long userId, long postId);
    Task<long> GetPostIdById(long id);
    Task<long> CountPostShares(long userId, long postId);
    Task DeletePostShareByPostId(long postId);
}