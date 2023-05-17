using Dapper;
using NewPoint.Handlers;
using NewPoint.Models;

namespace NewPoint.Repositories;

public class PostRepository : IPostRepository
{
    public async Task<List<Post>> GetPosts()
    {
        var reader = await DatabaseHandler.Connection.QueryMultipleAsync(@"
        SELECT * FROM ""post"";
        ");
        var posts = await reader.ReadAsync<Post>();
        return posts.ToList();
    }
}