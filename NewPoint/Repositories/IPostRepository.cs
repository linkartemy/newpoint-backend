using NewPoint.Models;

namespace NewPoint.Repositories;

public interface IPostRepository
{
    Task<IEnumerable<Post>> GetPosts();
}