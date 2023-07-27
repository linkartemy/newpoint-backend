using NewPoint.Models;

namespace NewPoint.Repositories;

public interface IPostRepository
{
    Task<IEnumerable<Post>> GetPosts();
    Task<Post> GetPost(long postId);
    Task<bool> IsLikedByUser(long postId, long userId);
    Task<long> GetLikesById(long postId);
    Task SetLikesById(long postId, long likes);
    Task<long> InsertPostLike(long postId, long userId);
    Task DeletePostLike(long postId, long userId);
    Task<long> GetSharesById(long postId);
    Task SetSharesById(long postId, long shares);
    Task<IEnumerable<Comment>> GetCommentsById(long postId);
}
