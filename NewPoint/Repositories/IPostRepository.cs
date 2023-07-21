using NewPoint.Models;

namespace NewPoint.Repositories;

public interface IPostRepository
{
    Task<IEnumerable<Post>> GetPosts();
    Task<Post> GetPost(long id);
    Task<bool> IsLikedByUser(long id, long userId);
}