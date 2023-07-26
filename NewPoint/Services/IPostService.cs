using NewPoint.Models;

namespace NewPoint.Services;

public interface IPostService
{
    Task<IEnumerable<Post>> GetPosts();
    Task<Post> GetPost(long id);
    Task<bool> IsLikedByUser(long id, long userId);
    Task Like(long id, long userId);
    Task UnLike(long id, long userId);
    Task Share(long id, long userId);
}