using NewPoint.Models;

namespace NewPoint.Repositories;

public interface IPostRepository
{
    Task<List<Post>> GetPosts();
}