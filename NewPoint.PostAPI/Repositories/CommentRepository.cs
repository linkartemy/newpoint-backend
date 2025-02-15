using System.Data;
using Dapper;
using NewPoint.Common.Handlers;
using NewPoint.Common.Models;

namespace NewPoint.PostAPI.Repositories;

public class CommentRepository : ICommentRepository
{
    public readonly string TableName = @"""comment""";
    public readonly string LikeTableName = @"""comment_like""";

    public async Task<long> Insert(long postId, long userId, string content)
    {
        var id = await DatabaseHandler.Connection.ExecuteScalarAsync<long>(@$"
        INSERT INTO {TableName} (post_id, user_id, content)
        VALUES (@postId, @userId, @content)
        RETURNING id;
        ",
            new
            {
                postId,
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

    public async Task<IEnumerable<Comment?>> GetCommentsByPostId(long postId)
    {
        var comments = await DatabaseHandler.Connection.QueryAsync<Comment?>(@$"
        SELECT 
            *
        FROM {TableName}
        WHERE post_id=@postId;
        ",
            new { postId });
        return comments;
    }

    public async Task<IEnumerable<Comment?>> GetCommentsByPostIdPaginated(long postId, int pageSize, DateTime? cursorCreatedAt, long? cursorId)
    {
        var limit = pageSize + 1;

        var sql = @$"
        SELECT *
        FROM {TableName}
        WHERE post_id = @PostId
        AND (@CursorCreatedAt IS NULL OR creation_timestamp < @CursorCreatedAt OR 
            (creation_timestamp = @CursorCreatedAt AND id < @CursorId))
        ORDER BY creation_timestamp DESC, id DESC
        LIMIT @Limit;";

        var parameters = new DynamicParameters();
        parameters.Add("PostId", postId, DbType.Int64);
        parameters.Add("CursorCreatedAt", cursorCreatedAt, DbType.DateTime);
        parameters.Add("CursorId", cursorId ?? long.MaxValue, DbType.Int64);
        parameters.Add("Limit", limit, DbType.Int32);

        return await DatabaseHandler.Connection.QueryAsync<Comment?>(sql, parameters);
    }


    public async Task<Comment?> GetCommentById(long commentId)
    {
        var comment = await DatabaseHandler.Connection.QueryFirstOrDefaultAsync<Comment?>(@$"
        SELECT 
            id AS id,
            user_id AS UserId,
            post_id AS PostId,
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

public interface ICommentRepository
{
    Task<long> Insert(long postId, long userId, string content);
    Task Delete(long commentId);
    Task<IEnumerable<Comment?>> GetCommentsByPostId(long postId);
    Task<IEnumerable<Comment?>> GetCommentsByPostIdPaginated(long postId, int pageSize, DateTime? cursorCreatedAt, long? cursorId);
    Task<Comment?> GetCommentById(long commentId);
    Task<bool> IsLikedByUser(long commentId, long userId);
    Task<long> GetLikesById(long commentId);
    Task SetLikesById(long commentId, long likes);
    Task<long> InsertCommentLike(long commentId, long userId);
    Task DeleteCommentLike(long commentId, long userId);
    Task DeleteCommentLikes(long commentId);
}