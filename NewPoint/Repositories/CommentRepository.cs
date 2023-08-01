using Dapper;
using NewPoint.Handlers;
using NewPoint.Models;

namespace NewPoint.Repositories;

public class CommentRepository : ICommentRepository
{
    public async Task<long> Insert(long postId, long userId, string content)
    {
        var id = await DatabaseHandler.Connection.ExecuteScalarAsync<long>(@"
        INSERT INTO ""comment"" (post_id, user_id, content)
        VALUES (@postId, @userId, @content)
        RETURNING id;
        ",
            new
            {
                postId, userId, content
            });
        return id;
    }

    public async Task<IEnumerable<Comment>> GetCommentsByPostId(long postId)
    {
        var comments = await DatabaseHandler.Connection.QueryAsync<Comment>(@"
        SELECT 
            id AS id,
            user_id AS UserId,
            post_id AS PostId,
            content AS Content,
            likes AS Likes,
            creation_timestamp as CreationTimestamp
        FROM ""comment""
        WHERE post_id=@postId;
        ",
            new { postId });
        return comments;
    }

    public async Task<bool> IsLikedByUser(long commentId, long userId)
    {
        var counter = await DatabaseHandler.Connection.ExecuteScalarAsync<int>(@"
        SELECT COUNT(1) FROM ""comment_like""
        WHERE
            comment_id=@commentId AND
            user_id=@userId;
        ",
            new { commentId, userId });

        return counter != 0;
    }

    public async Task<long> GetLikesById(long commentId)
    {
        var likes = await DatabaseHandler.Connection.QueryFirstOrDefaultAsync<long>(@"
        SELECT
            likes
        FROM ""comment""
        WHERE id=@commentId;
        ",
            new { commentId });
        return likes;
    }

    public async Task SetLikesById(long commentId, long likes)
    {
        await DatabaseHandler.Connection.ExecuteScalarAsync(@"
        UPDATE
            ""comment""
        SET likes=@likes
        WHERE id=@commentId;
        ",
            new { commentId, likes });
    }

    public async Task<long> InsertCommentLike(long commentId, long userId)
    {
        var likeId = await DatabaseHandler.Connection.ExecuteScalarAsync<long>(@"
        INSERT INTO ""comment_like"" (comment_id, user_id)
        VALUES (@commentId, @userId)
        RETURNING id;
        ",
            new
            {
                commentId, userId
            });
        return likeId;
    }

    public async Task DeleteCommentLike(long commentId, long userId)
    {
        await DatabaseHandler.Connection.ExecuteScalarAsync(@"
        DELETE FROM ""comment_like""
        WHERE comment_id=@commentId AND user_id=@userId;
        ",
            new
            {
                commentId, userId
            });
    }
}

public interface ICommentRepository
{
    Task<long> Insert(long postId, long userId, string content);
    Task<IEnumerable<Comment>> GetCommentsByPostId(long postId);
    Task<bool> IsLikedByUser(long commentId, long userId);
    Task<long> GetLikesById(long commentId);
    Task SetLikesById(long commentId, long likes);
    Task<long> InsertCommentLike(long commentId, long userId);
    Task DeleteCommentLike(long commentId, long userId);
}