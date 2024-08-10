using Dapper;
using NewPoint.Handlers;
using NewPoint.Models;

namespace NewPoint.Repositories;

public class PostBookmarkRepository : IPostBookmarkRepository
{
    private static string TableName => "post_bookmark";

    public async Task<long> AddPostBookmark(long userId, long postId)
    {
        var id = await DatabaseHandler.Connection.ExecuteScalarAsync<long>($@"
        INSERT INTO {TableName} (user_id, post_id, creation_timestamp)
        VALUES (@userId, @postId, now())
        RETURNING id;
        ",
            new
            {
                userId = userId,
                postId = postId
            });
        return id;
    }

    public async Task<int> CountPostBookmarks(long userId, long postId)
    {
        var count = await DatabaseHandler.Connection.ExecuteScalarAsync<int>($@"
        SELECT COUNT(*) FROM {TableName}
        WHERE user_id = @userId AND post_id = @postId;
        ",
            new
            {
                userId = userId,
                postId = postId
            });
        return count;
    }

    public async Task<IEnumerable<Bookmark>> GetPostBookmarksByUserIdFromId(long userId, long id)
    {
        var reader = await DatabaseHandler.Connection.QueryAsync<Bookmark>($@"
        SELECT 
            id AS Id,
            user_id AS UserId,
            post_id AS ItemId,
            creation_timestamp as CreationTimestamp
        FROM {TableName}
        WHERE user_id=@userId AND id <= @id
        ORDER BY id DESC
        LIMIT 10;
        ", new
        {
            userId,
            id
        });
        return reader;
    }

    public async Task<long> GetMaxId()
    {
        var id = await DatabaseHandler.Connection.ExecuteScalarAsync<long>($@"
        SELECT MAX(id) FROM {TableName};
        ", new
        {
        });
        return id;
    }

    public async Task DeletePostBookmark(long userId, long postId)
    {
        await DatabaseHandler.Connection.ExecuteAsync($@"
        DELETE FROM {TableName}
        WHERE user_id = @userId AND post_id = @postId;
        ",
            new
            {
                userId = userId,
                postId = postId
            });
    }

    public async Task DeleteAllBookmarksByUserId(long userId)
    {
        await DatabaseHandler.Connection.ExecuteAsync($@"
        DELETE FROM {TableName}
        WHERE user_id = @userId;
        ",
            new
            {
                userId = userId
            });
    }
}

public interface IPostBookmarkRepository
{
    Task<long> AddPostBookmark(long userId, long postId);
    Task<int> CountPostBookmarks(long userId, long postId);
    Task<IEnumerable<Bookmark>> GetPostBookmarksByUserIdFromId(long userId, long id);
    Task<long> GetMaxId();
    Task DeletePostBookmark(long userId, long postId);
    Task DeleteAllBookmarksByUserId(long userId);
}