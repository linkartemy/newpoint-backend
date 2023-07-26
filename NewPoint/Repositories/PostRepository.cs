using Dapper;
using NewPoint.Handlers;
using NewPoint.Models;

namespace NewPoint.Repositories;

public class PostRepository : IPostRepository
{
    public async Task<IEnumerable<Post>> GetPosts()
    {
        var reader = await DatabaseHandler.Connection.QueryAsync<Post>(@"
        SELECT 
            id AS id,
            author_id AS AuthorId,
            content AS Content,
            images AS Images,
            likes AS Likes,
            shares AS Shares,
            comments AS Comments,
            creation_timestamp as CreationTimestamp
        FROM ""post"";
        ");
        return reader;
    }

    public async Task<Post> GetPost(long postId)
    {
        var post = await DatabaseHandler.Connection.QueryFirstAsync<Post>(@"
        SELECT 
            id AS id,
            author_id AS AuthorId,
            content AS Content,
            images AS Images,
            likes AS Likes,
            shares AS Shares,
            comments AS Comments,
            creation_timestamp as CreationTimestamp
        FROM ""post""
        WHERE id=@postId;
        ",
            new { postId });
        return post;
    }

    public async Task<bool> IsLikedByUser(long postId, long userId)
    {
        var counter = await DatabaseHandler.Connection.ExecuteScalarAsync<int>(@"
        SELECT COUNT(1) FROM ""post_like""
        WHERE
            post_id=@postId AND
            user_id=@userId;
        ",
            new { postId, userId });

        return counter != 0;
    }

    public async Task<long> GetLikesById(long postId)
    {
        var likes = await DatabaseHandler.Connection.QueryFirstOrDefaultAsync<long>(@"
        SELECT
            likes
        FROM ""post""
        WHERE id=@postId;
        ",
            new { postId });
        return likes;
    }

    public async Task SetLikesById(long postId, long likes)
    {
        await DatabaseHandler.Connection.ExecuteScalarAsync(@"
        UPDATE
            ""post""
        SET likes=@likes
        WHERE id=@postId;
        ",
            new { postId, likes });
    }

    public async Task<long> InsertPostLike(long postId, long userId)
    {
        var likeId = await DatabaseHandler.Connection.ExecuteScalarAsync<long>(@"
        INSERT INTO ""post_like"" (post_id, user_id)
        VALUES (@postId, @userId)
        RETURNING id;
        ",
            new
            {
                postId, userId
            });
        return likeId;
    }

    public async Task DeletePostLike(long postId, long userId)
    {
        await DatabaseHandler.Connection.ExecuteScalarAsync(@"
        DELETE FROM ""post_like""
        WHERE post_id=@postId AND user_id=@userId;
        ",
            new
            {
                postId, userId
            });
    }
    
    public async Task<long> GetSharesById(long postId)
    {
        var shares = await DatabaseHandler.Connection.QueryFirstOrDefaultAsync<long>(@"
        SELECT
            shares
        FROM ""post""
        WHERE id=@postId;
        ",
            new { postId });
        return shares;
    }

    public async Task SetSharesById(long postId, long shares)
    {
        await DatabaseHandler.Connection.ExecuteScalarAsync(@"
        UPDATE
            ""post""
        SET shares=@shares
        WHERE id=@postId;
        ",
            new { postId, shares });
    }
}