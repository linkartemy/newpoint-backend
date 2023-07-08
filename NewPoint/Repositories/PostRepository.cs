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
            creation_timestamp as CreationTimestamp
        FROM ""post"";
        ");
        return reader;
    }
}