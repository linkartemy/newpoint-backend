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
            id AS Id,
            author_id AS AuthorId,
            content AS Content,
            images AS Images,
            likes AS Likes,
            shares AS Shares,
            comments AS Comments,
            views AS Views,
            creation_timestamp as CreationTimestamp
        FROM ""post"";
        ");
        return reader;
    }

    public async Task<IEnumerable<Post>> GetPostsByAuthorId(long authorId)
    {
        var reader = await DatabaseHandler.Connection.QueryAsync<Post>(@"
        SELECT 
            id AS Id,
            author_id AS AuthorId,
            content AS Content,
            images AS Images,
            likes AS Likes,
            shares AS Shares,
            comments AS Comments,
            views AS Views,
            creation_timestamp as CreationTimestamp
        FROM ""post""
        WHERE author_id=@authorId;
        ",
            new { authorId });
        return reader;
    }

    public async Task<Post> GetPost(long postId)
    {
        var post = await DatabaseHandler.Connection.QueryFirstAsync<Post>(@"
        SELECT 
            id AS Id,
            author_id AS AuthorId,
            content AS Content,
            images AS Images,
            likes AS Likes,
            shares AS Shares,
            comments AS Comments,
            views AS Views,
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

    public async Task<int> GetLikesById(long postId)
    {
        var likes = await DatabaseHandler.Connection.QueryFirstOrDefaultAsync<int>(@"
        SELECT
            likes
        FROM ""post""
        WHERE id=@postId;
        ",
            new { postId });
        return likes;
    }

    public async Task SetLikesById(long postId, int likes)
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
                postId,
                userId
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
                postId,
                userId
            });
    }

    public async Task<int> GetSharesById(long postId)
    {
        var shares = await DatabaseHandler.Connection.QueryFirstOrDefaultAsync<int>(@"
        SELECT
            shares
        FROM ""post""
        WHERE id=@postId;
        ",
            new { postId });
        return shares;
    }

    public async Task SetSharesById(long postId, int shares)
    {
        await DatabaseHandler.Connection.ExecuteScalarAsync(@"
        UPDATE
            ""post""
        SET shares=@shares
        WHERE id=@postId;
        ",
            new { postId, shares });
    }

    public async Task<int> GetCommentsById(long postId)
    {
        var comments = await DatabaseHandler.Connection.QueryFirstOrDefaultAsync<int>(@"
        SELECT
            comments
        FROM ""post""
        WHERE id=@postId;
        ",
            new { postId });
        return comments;
    }

    public async Task SetCommentsById(long postId, int comments)
    {
        await DatabaseHandler.Connection.ExecuteScalarAsync(@"
        UPDATE
            ""post""
        SET comments=@comments
        WHERE id=@postId;
        ",
            new { postId, comments });
    }

    public async Task<int> GetPostViewsById(long postId)
    {
        var views = await DatabaseHandler.Connection.QueryFirstOrDefaultAsync<int>(@"
        SELECT
            views
        FROM ""post""
        WHERE id=@postId;
        ",
            new { postId });
        return views;
    }

    public async Task SetPostViewsById(long postId, int views)
    {
        await DatabaseHandler.Connection.ExecuteScalarAsync(@"
        UPDATE
            ""post""
        SET views=@views
        WHERE id=@postId;
        ",
            new { postId, views });
    }
}

public interface IPostRepository
{
    Task<IEnumerable<Post>> GetPosts();
    Task<IEnumerable<Post>> GetPostsByAuthorId(long authorId);
    Task<Post> GetPost(long postId);
    Task<bool> IsLikedByUser(long postId, long userId);
    Task<int> GetLikesById(long postId);
    Task SetLikesById(long postId, int likes);
    Task<long> InsertPostLike(long postId, long userId);
    Task DeletePostLike(long postId, long userId);
    Task<int> GetSharesById(long postId);
    Task SetSharesById(long postId, int shares);
    Task<int> GetCommentsById(long postId);
    Task SetCommentsById(long postId, int comments);
    Task<int> GetPostViewsById(long postId);
    Task SetPostViewsById(long postId, int views);
}