﻿using Dapper;
using NewPoint.Handlers;
using NewPoint.Models;

namespace NewPoint.Repositories;

public class PostRepository : IPostRepository
{
    public readonly string TableName = @"""post""";
    public readonly string LikeTableName = @"""post_like""";

    public async Task<long> AddPost(long authorId, string content)
    {
        var id = await DatabaseHandler.Connection.ExecuteScalarAsync<long>(@$"
        INSERT INTO {TableName} (author_id, content, creation_timestamp)
        VALUES (@authorId, @content, now())
        RETURNING id;
        ",
            new
            {
                authorId = authorId,
                content = content
            });
        return id;
    }

    public async Task<IEnumerable<Post>> GetPosts()
    {
        var reader = await DatabaseHandler.Connection.QueryAsync<Post>(@$"
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
        FROM {TableName};
        ");
        return reader;
    }

    public async Task<IEnumerable<Post>> GetPostsByAuthorId(long authorId)
    {
        var reader = await DatabaseHandler.Connection.QueryAsync<Post>(@$"
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
        FROM {TableName}
        WHERE author_id=@authorId;
        ",
            new { authorId });
        return reader;
    }

    public async Task<IEnumerable<Post>> GetPostsFromId(long id)
    {
        var reader = await DatabaseHandler.Connection.QueryAsync<Post>(@$"
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
        FROM {TableName}
        WHERE id <= @id
        ORDER BY id DESC
        LIMIT 10
        ",
            new { id });
        return reader;
    }

    public async Task<long> GetMaxId()
    {
        var id = await DatabaseHandler.Connection.QueryFirstOrDefaultAsync<long>(@$"
        SELECT MAX(id) FROM {TableName};");
        return id;
    }

    public async Task<Post?> GetPost(long postId)
    {
        var post = await DatabaseHandler.Connection.QueryFirstOrDefaultAsync<Post?>(@"
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
        FROM { TableName}
        WHERE id = @postId;
        ",
            new { postId });
        return post;
    }

    public async Task<bool> IsLikedByUser(long postId, long userId)
    {
        var counter = await DatabaseHandler.Connection.ExecuteScalarAsync<int>(@$"
        SELECT COUNT(1) FROM {LikeTableName}
        WHERE
            post_id=@postId AND
            user_id=@userId;
        ",
            new { postId, userId });

        return counter != 0;
    }

    public async Task<int> GetLikesById(long postId)
    {
        var likes = await DatabaseHandler.Connection.QueryFirstOrDefaultAsync<int>(@$"
        SELECT
            likes
        FROM {TableName}
        WHERE id=@postId;
        ",
            new { postId });
        return likes;
    }

    public async Task SetLikesById(long postId, int likes)
    {
        await DatabaseHandler.Connection.ExecuteScalarAsync(@$"
        UPDATE
            {TableName}
        SET likes=@likes
        WHERE id=@postId;
        ",
            new { postId, likes });
    }

    public async Task<long> InsertPostLike(long postId, long userId)
    {
        var likeId = await DatabaseHandler.Connection.ExecuteScalarAsync<long>(@$"
        INSERT INTO {LikeTableName} (post_id, user_id)
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
        await DatabaseHandler.Connection.ExecuteScalarAsync(@$"
        DELETE FROM {LikeTableName}
        WHERE post_id=@postId AND user_id=@userId;
        ",
            new
            {
                postId,
                userId
            });
    }

    public async Task DeletePostLikes(long postId)
    {
        await DatabaseHandler.Connection.ExecuteScalarAsync(@$"
        DELETE FROM {LikeTableName}
        WHERE post_id=@postId;
        ",
            new
            {
                postId
            });
    }

    public async Task<int> GetSharesById(long postId)
    {
        var shares = await DatabaseHandler.Connection.QueryFirstOrDefaultAsync<int>(@$"
        SELECT
            shares
        FROM {TableName}
        WHERE id=@postId;
        ",
            new { postId });
        return shares;
    }

    public async Task SetSharesById(long postId, int shares)
    {
        await DatabaseHandler.Connection.ExecuteScalarAsync(@$"
        UPDATE
            {TableName}
        SET shares=@shares
        WHERE id=@postId;
        ",
            new { postId, shares });
    }

    public async Task<int> GetCommentsById(long postId)
    {
        var comments = await DatabaseHandler.Connection.QueryFirstOrDefaultAsync<int>(@$"
        SELECT
            comments
        FROM {TableName}
        WHERE id=@postId;
        ",
            new { postId });
        return comments;
    }

    public async Task SetCommentsById(long postId, int comments)
    {
        await DatabaseHandler.Connection.ExecuteScalarAsync(@$"
        UPDATE
            {TableName}
        SET comments=@comments
        WHERE id=@postId;
        ",
            new { postId, comments });
    }

    public async Task<int> GetPostViewsById(long postId)
    {
        var views = await DatabaseHandler.Connection.QueryFirstOrDefaultAsync<int>(@$"
        SELECT
            views
        FROM {TableName}
        WHERE id=@postId;
        ",
            new { postId });
        return views;
    }

    public async Task SetPostViewsById(long postId, int views)
    {
        await DatabaseHandler.Connection.ExecuteScalarAsync(@$"
        UPDATE
            {TableName}
        SET views=@views
        WHERE id=@postId;
        ",
            new { postId, views });
    }

    public async Task DeletePost(long postId)
    {
        await DatabaseHandler.Connection.ExecuteAsync(@$"
        DELETE FROM {TableName} WHERE id = @postId;
        ",
            new
            {
                postId = postId
            });
    }
}

public interface IPostRepository
{
    Task<long> AddPost(long authorId, string content);
    Task<IEnumerable<Post>> GetPosts();
    Task<IEnumerable<Post>> GetPostsByAuthorId(long authorId);
    Task<IEnumerable<Post>> GetPostsFromId(long id);
    Task<long> GetMaxId();
    Task<Post?> GetPost(long postId);
    Task<bool> IsLikedByUser(long postId, long userId);
    Task<int> GetLikesById(long postId);
    Task SetLikesById(long postId, int likes);
    Task<long> InsertPostLike(long postId, long userId);
    Task DeletePostLike(long postId, long userId);
    Task DeletePostLikes(long postId);
    Task<int> GetSharesById(long postId);
    Task SetSharesById(long postId, int shares);
    Task<int> GetCommentsById(long postId);
    Task SetCommentsById(long postId, int comments);
    Task<int> GetPostViewsById(long postId);
    Task SetPostViewsById(long postId, int views);
    Task DeletePost(long postId);
}