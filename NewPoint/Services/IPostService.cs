using NewPoint.Models;

namespace NewPoint.Services;

public interface IPostService
{
    Task<List<Post>> GetPosts();
}