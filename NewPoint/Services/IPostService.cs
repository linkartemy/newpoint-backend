using NewPoint.Models;

namespace NewPoint.Services;

public interface IPostService
{
    Task<List<Post>> GetPosts();
    Task<Post> GetPost(long id);
}