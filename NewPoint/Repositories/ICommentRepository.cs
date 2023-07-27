using NewPoint.Models;

namespace NewPoint.Repositories;

public interface ICommentRepository
{
    Task<IEnumerable<Comment>> GetCommentsByPostId(long postId);
    Task<bool> IsLikedByUser(long commentId, long userId);
    Task<long> GetLikesById(long commentId);
    Task SetLikesById(long commentId, long likes);
    Task<long> InsertCommentLike(long commentId, long userId);
    Task DeleteCommentLike(long commentId, long userId);
}
