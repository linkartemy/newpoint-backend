using Dapper;
using NewPoint.Handlers;
using NewPoint.Models;

namespace NewPoint.Repositories;

public class ArticleRepository : IArticleRepository
{
    public readonly string TableName = @"""article""";
    public readonly string LikeTableName = @"""article_like""";

    public async Task<long> AddArticle(long authorId, string title, string content)
    {
        var id = await DatabaseHandler.Connection.ExecuteScalarAsync<long>(@$"
        INSERT INTO {TableName} (author_id, title, content, creation_timestamp)
        VALUES (@authorId, @title, @content, now())
        RETURNING id;
        ",
            new
            {
                authorId = authorId,
                title = title,
                content = content,
            });
        return id;
    }

    public async Task<IEnumerable<Article>> GetArticles()
    {
        var reader = await DatabaseHandler.Connection.QueryAsync<Article>(@$"
        SELECT 
            id AS Id,
            author_id AS AuthorId,
            title AS Title,
            content AS Content,
            images AS Images,
            likes AS Likes,
            shares AS Shares,
            comments AS Comments,
            views AS Views,
            creation_timestamp as CreationTimestamp
        FROM {TableName};
        ");
        return reader;
    }

    public async Task<IEnumerable<Article>> GetArticlesFromId(long id)
    {
        var reader = await DatabaseHandler.Connection.QueryAsync<Article>(@$"
        SELECT 
            id AS Id,
            author_id AS AuthorId,
            title AS Title,
            content AS Content,
            images AS Images,
            likes AS Likes,
            shares AS Shares,
            comments AS Comments,
            views AS Views,
            creation_timestamp as CreationTimestamp
        FROM {TableName}
        WHERE id <= @id
        ORDER BY id DESC
        LIMIT 10;
        ",
            new { id });
        return reader;
    }

    public async Task<IEnumerable<Article>> GetArticlesByAuthorId(long authorId)
    {
        var reader = await DatabaseHandler.Connection.QueryAsync<Article>(@$"
        SELECT 
            id AS Id,
            author_id AS AuthorId,
            title AS Title,
            content AS Content,
            images AS Images,
            likes AS Likes,
            shares AS Shares,
            comments AS Comments,
            views AS Views,
            creation_timestamp as CreationTimestamp
        FROM {TableName}
        WHERE author_id=@authorId;
        ",
            new { authorId });
        return reader;
    }

    public async Task<long> GetMaxId()
    {
        var id = await DatabaseHandler.Connection.ExecuteScalarAsync<long>(@$"
        SELECT MAX(id) FROM {TableName};
        ");
        return id;
    }

    public async Task<Article> GetArticle(long articleId)
    {
        var article = await DatabaseHandler.Connection.QueryFirstAsync<Article>(@$"
        SELECT
            id AS Id,
            author_id AS AuthorId,
            title AS Title,
            content AS Content,
            images AS Images,
            likes AS Likes,
            shares AS Shares,
            comments AS Comments,
            views AS Views,
            creation_timestamp as CreationTimestamp
        FROM {TableName}
        WHERE id=@articleId;
        ",
            new { articleId });
        return article;
    }

    public async Task<bool> IsLikedByUser(long articleId, long userId)
    {
        var counter = await DatabaseHandler.Connection.ExecuteScalarAsync<int>(@$"
        SELECT COUNT(1) FROM {LikeTableName}
        WHERE
            article_id=@articleId AND
            user_id=@userId;
        ",
            new { articleId, userId });

        return counter != 0;
    }

    public async Task<int> GetLikesById(long articleId)
    {
        var likes = await DatabaseHandler.Connection.QueryFirstOrDefaultAsync<int>(@$"
        SELECT
            likes
        FROM {TableName}
        WHERE id=@articleId;
        ",
            new { articleId });
        return likes;
    }

    public async Task SetLikesById(long articleId, int likes)
    {
        await DatabaseHandler.Connection.ExecuteScalarAsync(@$"
        UPDATE
            {TableName}
        SET likes=@likes
        WHERE id=@articleId;
        ",
            new { articleId, likes });
    }

    public async Task<long> InsertArticleLike(long articleId, long userId)
    {
        var likeId = await DatabaseHandler.Connection.ExecuteScalarAsync<long>(@$"
        INSERT INTO {LikeTableName} (article_id, user_id)
        VALUES (@articleId, @userId)
        RETURNING id;
        ",
            new
            {
                articleId,
                userId
            });
        return likeId;
    }

    public async Task DeleteArticleLike(long articleId, long userId)
    {
        await DatabaseHandler.Connection.ExecuteScalarAsync(@$"
        DELETE FROM {LikeTableName}
        WHERE article_id=@articleId AND user_id=@userId;
        ",
            new
            {
                articleId,
                userId
            });
    }

    public async Task DeleteArticleLikes(long articleId)
    {
        await DatabaseHandler.Connection.ExecuteScalarAsync(@$"
        DELETE FROM {LikeTableName}
        WHERE article_id=@articleId;
        ",
            new
            {
                articleId
            });
    }

    public async Task<int> GetSharesById(long articleId)
    {
        var shares = await DatabaseHandler.Connection.QueryFirstOrDefaultAsync<int>(@$"
        SELECT
            shares
        FROM {TableName}
        WHERE id=@articleId;
        ",
            new { articleId });
        return shares;
    }

    public async Task SetSharesById(long articleId, int shares)
    {
        await DatabaseHandler.Connection.ExecuteScalarAsync(@$"
        UPDATE
            {TableName}
        SET shares=@shares
        WHERE id=@articleId;
        ",
            new { articleId, shares });
    }

    public async Task<int> GetCommentsById(long articleId)
    {
        var comments = await DatabaseHandler.Connection.QueryFirstOrDefaultAsync<int>(@$"
        SELECT
            comments
        FROM {TableName}
        WHERE id=@articleId;
        ",
            new { articleId });
        return comments;
    }

    public async Task SetCommentsById(long articleId, int comments)
    {
        await DatabaseHandler.Connection.ExecuteScalarAsync(@$"
        UPDATE
            {TableName}
        SET comments=@comments
        WHERE id=@articleId;
        ",
            new { articleId, comments });
    }

    public async Task<int> GetArticleViewsById(long articleId)
    {
        var views = await DatabaseHandler.Connection.QueryFirstOrDefaultAsync<int>(@$"
        SELECT
            views
        FROM {TableName}
        WHERE id=@articleId;
        ",
            new { articleId });
        return views;
    }

    public async Task SetArticleViewsById(long articleId, int views)
    {
        await DatabaseHandler.Connection.ExecuteScalarAsync(@$"
        UPDATE
            {TableName}
        SET views=@views
        WHERE id=@articleId;
        ",
            new { articleId, views });
    }

    public async Task DeleteArticle(long articleId)
    {
        await DatabaseHandler.Connection.ExecuteAsync(@$"
        DELETE FROM {TableName} WHERE id = @articleId;
        ",
            new
            {
                articleId = articleId
            });
    }
}

public interface IArticleRepository
{
    Task<long> AddArticle(long authorId, string title, string content);
    Task<IEnumerable<Article>> GetArticles();
    Task<IEnumerable<Article>> GetArticlesByAuthorId(long authorId);
    Task<IEnumerable<Article>> GetArticlesFromId(long id);
    Task<long> GetMaxId();
    Task<Article> GetArticle(long articleId);
    Task<bool> IsLikedByUser(long articleId, long userId);
    Task<int> GetLikesById(long articleId);
    Task SetLikesById(long articleId, int likes);
    Task<long> InsertArticleLike(long articleId, long userId);
    Task DeleteArticleLike(long articleId, long userId);
    Task DeleteArticleLikes(long articleId);
    Task<int> GetSharesById(long articleId);
    Task SetSharesById(long articleId, int shares);
    Task<int> GetCommentsById(long articleId);
    Task SetCommentsById(long articleId, int comments);
    Task<int> GetArticleViewsById(long articleId);
    Task SetArticleViewsById(long articleId, int views);
    Task DeleteArticle(long articleId);
}