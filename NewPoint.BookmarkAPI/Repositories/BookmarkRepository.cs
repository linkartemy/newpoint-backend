using System.Data;
using Dapper;
using NewPoint.Common.Handlers;
using NewPoint.Common.Models;

namespace NewPoint.BookmarkAPI.Repositories;

public class BookmarkRepository : IBookmarkRepository
{
    public readonly string PostTableName = @"""post_bookmark""";
    public readonly string ArticleTableName = @"""article_bookmark""";

    public async Task<long> AddPostBookmark(long userId, long postId)
    {
        var query = $@"
            INSERT INTO {PostTableName} (user_id, post_id)
            VALUES (@UserId, @PostId)
            RETURNING id";
        return await DatabaseHandler.Connection.ExecuteScalarAsync<long>(query, new { UserId = userId, PostId = postId });
    }

    public async Task DeletePostBookmark(long userId, long postId)
    {
        var query = $@"
            DELETE FROM {PostTableName}
            WHERE user_id = @UserId AND post_id = @PostId";
        await DatabaseHandler.Connection.ExecuteAsync(query, new { UserId = userId, PostId = postId });
    }

    public async Task<IEnumerable<long>> GetBookmarkedPostsPaginated(long userId, int pageSize, DateTime? cursorCreatedAt, long? cursorId)
    {
        var query = $@"
        SELECT post_id
        FROM {PostTableName}
        WHERE user_id = @UserId
        ORDER BY creation_timestamp DESC, id DESC
        LIMIT @PageSize";

        if (cursorCreatedAt.HasValue && cursorId.HasValue)
        {
            query += $@"
            OFFSET (
                SELECT COUNT(*)
                FROM {PostTableName}
                WHERE user_id = @UserId 
                AND (creation_timestamp, id) > (@CursorCreatedAt, @CursorId)
            )";
        }

        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId, DbType.Int64);
        parameters.Add("PageSize", pageSize, DbType.Int32);
        parameters.Add("CursorCreatedAt", cursorCreatedAt, DbType.DateTime);
        parameters.Add("CursorId", cursorId, DbType.Int64);

        return await DatabaseHandler.Connection.QueryAsync<long>(query, parameters);
    }


    public async Task<bool> IsPostBookmarked(long userId, long postId)
    {
        var query = $@"
            SELECT EXISTS (
                SELECT 1
                FROM {PostTableName}
                WHERE user_id = @UserId AND post_id = @PostId)";
        return await DatabaseHandler.Connection.ExecuteScalarAsync<bool>(query, new { UserId = userId, PostId = postId });
    }

    public async Task<int> GetBookmarksCount(long postId)
    {
        var query = $@"
            SELECT COUNT(*)
            FROM {PostTableName}
            WHERE post_id = @PostId";
        return await DatabaseHandler.Connection.ExecuteScalarAsync<int>(query, new { PostId = postId });
    }

    public async Task<int> GetBookmarksCountByUserId(long userId)
    {
        var query = $@"
            SELECT COUNT(*)
            FROM {PostTableName}
            WHERE user_id = @UserId";
        return await DatabaseHandler.Connection.ExecuteScalarAsync<int>(query, new { UserId = userId });
    }

    public async Task<long> AddArticleBookmark(long userId, long articleId)
    {
        var query = $@"
            INSERT INTO {ArticleTableName} (user_id, article_id)
            VALUES (@UserId, @ArticleId)
            RETURNING id";
        return await DatabaseHandler.Connection.ExecuteScalarAsync<long>(query, new { UserId = userId, ArticleId = articleId });
    }

    public async Task DeleteArticleBookmark(long userId, long articleId)
    {
        var query = $@"
            DELETE FROM {ArticleTableName}
            WHERE user_id = @UserId AND article_id = @ArticleId";
        await DatabaseHandler.Connection.ExecuteAsync(query, new { UserId = userId, ArticleId = articleId });
    }

    public async Task<IEnumerable<long>> GetBookmarkedArticlesPaginated(long userId, int pageSize, DateTime? cursorCreatedAt, long? cursorId)
    {
        var query = $@"
        SELECT article_id
        FROM {ArticleTableName}
        WHERE user_id = @UserId
        ORDER BY creation_timestamp DESC, id DESC
        LIMIT @PageSize";

        if (cursorCreatedAt.HasValue && cursorId.HasValue)
        {
            query += $@"
            OFFSET (
                SELECT COUNT(*)
                FROM {ArticleTableName}
                WHERE user_id = @UserId 
                AND (creation_timestamp, id) > (@CursorCreatedAt, @CursorId)
            )";
        }

        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId, DbType.Int64);
        parameters.Add("PageSize", pageSize, DbType.Int32);
        parameters.Add("CursorCreatedAt", cursorCreatedAt, DbType.DateTime);
        parameters.Add("CursorId", cursorId, DbType.Int64);

        return await DatabaseHandler.Connection.QueryAsync<long>(query, parameters);
    }


    public async Task<bool> IsArticleBookmarked(long userId, long articleId)
    {
        var query = $@"
            SELECT EXISTS (
                SELECT 1
                FROM {ArticleTableName}
                WHERE user_id = @UserId AND article_id = @ArticleId)";
        return await DatabaseHandler.Connection.ExecuteScalarAsync<bool>(query, new { UserId = userId, ArticleId = articleId });
    }

    public async Task<int> GetArticleBookmarksCount(long articleId)
    {
        var query = $@"
            SELECT COUNT(*)
            FROM {ArticleTableName}
            WHERE article_id = @ArticleId";
        return await DatabaseHandler.Connection.ExecuteScalarAsync<int>(query, new { ArticleId = articleId });
    }

    public async Task<int> GetArticleBookmarksCountByUserId(long userId)
    {
        var query = $@"
            SELECT COUNT(*)
            FROM {ArticleTableName}
            WHERE user_id = @UserId";
        return await DatabaseHandler.Connection.ExecuteScalarAsync<int>(query, new { UserId = userId });
    }

    public async Task DeleteAllBookmarksByUserId(long userId)
    {
        var query = $@"
            DELETE FROM {PostTableName}
            WHERE user_id = @UserId;
            DELETE FROM {ArticleTableName}
            WHERE user_id = @UserId;";
        await DatabaseHandler.Connection.ExecuteAsync(query, new { UserId = userId });
    }
}

public interface IBookmarkRepository
{
    Task<long> AddPostBookmark(long userId, long postId);
    Task DeletePostBookmark(long userId, long postId);
    Task<IEnumerable<long>> GetBookmarkedPostsPaginated(long userId, int pageSize, DateTime? cursorCreatedAt, long? cursorId);
    Task<bool> IsPostBookmarked(long userId, long postId);
    Task<int> GetBookmarksCount(long postId);
    Task<int> GetBookmarksCountByUserId(long userId);
    Task<long> AddArticleBookmark(long userId, long articleId);
    Task DeleteArticleBookmark(long userId, long articleId);
    Task<IEnumerable<long>> GetBookmarkedArticlesPaginated(long userId, int pageSize, DateTime? cursorCreatedAt, long? cursorId);
    Task<bool> IsArticleBookmarked(long userId, long articleId);
    Task<int> GetArticleBookmarksCount(long articleId);
    Task<int> GetArticleBookmarksCountByUserId(long userId);
    Task DeleteAllBookmarksByUserId(long userId);
}