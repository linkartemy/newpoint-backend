using NewPoint.Models;

namespace NewPoint.Repositories;

public interface IPostRepository
{
    Task<IEnumerable<Post>> GetPosts();
    Task<Post> GetPost(long postId);
    Task<bool> IsLikedByUser(long postId, long userId);
    Task<int> GetLikesById(long postId);
    Task SetLikesById(long postId, int likes);
    Task<long> InsertPostLike(long postId, long userId);
    Task DeletePostLike(long postId, long userId);
    Task<int> GetSharesById(long postId);
    Task SetSharesById(long postId, int shares);
    Task<int> GetCommentsById(long postId);
    Task SetCommentsById(long postId, int comments);
}
