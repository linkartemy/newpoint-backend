using Dapper;
using NewPoint.Handlers;
using NewPoint.Models;

namespace NewPoint.Repositories;

public class ArticleBookmarkRepository : IArticleBookmarkRepository
{
    private static string TableName => "article_bookmark";

    public async Task<long> AddArticleBookmark(long userId, long articleId)
    {
        var id = await DatabaseHandler.Connection.ExecuteScalarAsync<long>($@"
        INSERT INTO {TableName} (user_id, article_id, creation_timestamp)
        VALUES (@userId, @articleId, now())
        RETURNING id;
        ",
            new
            {
                userId = userId,
                articleId = articleId
            });
        return id;
    }

    public async Task<int> CountArticleBookmarks(long userId, long articleId)
    {
        var count = await DatabaseHandler.Connection.ExecuteScalarAsync<int>($@"
        SELECT COUNT(*) FROM {TableName}
        WHERE user_id = @userId AND article_id = @articleId;
        ",
            new
            {
                userId = userId,
                articleId = articleId
            });
        return count;
    }

    public async Task<IEnumerable<Bookmark>> GetArticleBookmarksByUserIdFromId(long userId, long id)
    {
        var reader = await DatabaseHandler.Connection.QueryAsync<Bookmark>($@"
        SELECT 
            id AS Id,
            user_id AS UserId,
            article_id AS ItemId,
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

    public async Task DeleteArticleBookmark(long userId, long articleId)
    {
        await DatabaseHandler.Connection.ExecuteAsync($@"
        DELETE FROM {TableName}
        WHERE user_id = @userId AND article_id = @articleId;
        ",
            new
            {
                userId = userId,
                articleId = articleId
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

public interface IArticleBookmarkRepository
{
    Task<long> AddArticleBookmark(long userId, long articleId);
    Task<int> CountArticleBookmarks(long userId, long articleId);
    Task<IEnumerable<Bookmark>> GetArticleBookmarksByUserIdFromId(long userId, long id);
    Task<long> GetMaxId();
    Task DeleteArticleBookmark(long userId, long articleId);
    Task DeleteAllBookmarksByUserId(long userId);
}