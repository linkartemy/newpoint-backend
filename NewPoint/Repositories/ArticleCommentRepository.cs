using Dapper;
using NewPoint.Handlers;
using NewPoint.Models;

namespace NewPoint.Repositories;

public class ArticleCommentRepository : IArticleCommentRepository
{
    public readonly string TableName = "article_comment";
    public readonly string LikeTableName = "article_comment_like";

    public async Task<long> Insert(long articleId, long userId, string content)
    {
        var id = await DatabaseHandler.Connection.ExecuteScalarAsync<long>(@$"
        INSERT INTO {TableName} (article_id, user_id, content)
        VALUES (@articleId, @userId, @content)
        RETURNING id;
        ",
            new
            {
                articleId,
                userId,
                content
            });
        return id;
    }

    public async Task Delete(long commentId)
    {
        await DatabaseHandler.Connection.ExecuteAsync(@$"
        DELETE FROM {TableName} WHERE id = @commentId;
        ",
            new
            {
                commentId = commentId
            });
    }

    public async Task<IEnumerable<ArticleComment?>> GetCommentsByArticleId(long articleId)
    {
        var comments = await DatabaseHandler.Connection.QueryAsync<ArticleComment?>(@$"
        SELECT 
            *
        FROM {TableName}
        WHERE article_id=@articleId;
        ",
            new { articleId });
        return comments;
    }

    public async Task<ArticleComment?> GetCommentById(long commentId)
    {
        var comment = await DatabaseHandler.Connection.QueryFirstOrDefaultAsync<ArticleComment?>(@$"
        SELECT 
            id AS id,
            user_id AS UserId,
            article_id AS PostId,
            content AS Content,
            likes AS Likes,
            creation_timestamp as CreationTimestamp
        FROM {TableName}
        WHERE id=@commentId;
        ",
            new { commentId });
        return comment;
    }

    public async Task<bool> IsLikedByUser(long commentId, long userId)
    {
        var counter = await DatabaseHandler.Connection.ExecuteScalarAsync<int>(@$"
        SELECT COUNT(1) FROM {LikeTableName}
        WHERE
            comment_id=@commentId AND
            user_id=@userId;
        ",
            new { commentId, userId });

        return counter != 0;
    }

    public async Task<long> GetLikesById(long commentId)
    {
        var likes = await DatabaseHandler.Connection.QueryFirstOrDefaultAsync<long>(@$"
        SELECT
            likes
        FROM {TableName}
        WHERE id=@commentId;
        ",
            new { commentId });
        return likes;
    }

    public async Task SetLikesById(long commentId, long likes)
    {
        await DatabaseHandler.Connection.ExecuteScalarAsync(@$"
        UPDATE
            {TableName}
        SET likes=@likes
        WHERE id=@commentId;
        ",
            new { commentId, likes });
    }

    public async Task<long> InsertCommentLike(long commentId, long userId)
    {
        var likeId = await DatabaseHandler.Connection.ExecuteScalarAsync<long>(@$"
        INSERT INTO {LikeTableName} (comment_id, user_id)
        VALUES (@commentId, @userId)
        RETURNING id;
        ",
            new
            {
                commentId,
                userId
            });
        return likeId;
    }

    public async Task DeleteCommentLike(long commentId, long userId)
    {
        await DatabaseHandler.Connection.ExecuteScalarAsync(@$"
        DELETE FROM {LikeTableName}
        WHERE comment_id=@commentId AND user_id=@userId;
        ",
            new
            {
                commentId,
                userId
            });
    }

    public async Task DeleteCommentLikes(long commentId)
    {
        await DatabaseHandler.Connection.ExecuteScalarAsync(@$"
        DELETE FROM {LikeTableName}
        WHERE comment_id=@commentId;
        ",
            new
            {
                commentId
            });
    }
}

public interface IArticleCommentRepository
{
    Task<long> Insert(long articleId, long userId, string content);
    Task Delete(long commentId);
    Task<IEnumerable<ArticleComment?>> GetCommentsByArticleId(long articleId);
    Task<ArticleComment?> GetCommentById(long commentId);
    Task<bool> IsLikedByUser(long commentId, long userId);
    Task<long> GetLikesById(long commentId);
    Task SetLikesById(long commentId, long likes);
    Task<long> InsertCommentLike(long commentId, long userId);
    Task DeleteCommentLike(long commentId, long userId);
    Task DeleteCommentLikes(long commentId);
}