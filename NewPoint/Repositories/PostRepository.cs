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

    public async Task<Post> GetPost(long id)
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
        WHERE id=@id;
        ",
            new { id });
        return post;
    }

    public async Task<bool> IsLikedByUser(long id, long userId)
    {
        var counter = await DatabaseHandler.Connection.ExecuteScalarAsync<int>(@"
        SELECT COUNT(1) FROM ""post_like""
        WHERE
            post_id=@id AND
            user_id=@userId;
        ",
            new { id, userId });

        return counter != 0;
    }
}